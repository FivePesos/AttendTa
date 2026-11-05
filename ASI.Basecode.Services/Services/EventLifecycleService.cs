using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Draft Event creation and editing. Every write locks the Event Type, then the Venue, then the
    /// Event, in the order AD-4 requires, and never converts time zones (AD-11).
    /// </summary>
    public class EventLifecycleService : IEventLifecycleService
    {
        private readonly IEventRepository _events;
        private readonly IEventTypeRepository _eventTypes;
        private readonly IVenueRepository _venues;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly IMapper _mapper;
        private readonly IActivityLogWriter _audit;
        private readonly ICurrentActor _actor;

        public EventLifecycleService(
            IEventRepository events,
            IEventTypeRepository eventTypes,
            IVenueRepository venues,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TimeProvider timeProvider,
            IActivityLogWriter audit,
            ICurrentActor actor)
        {
            _events = events;
            _eventTypes = eventTypes;
            _venues = venues;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _timeProvider = timeProvider;
            _audit = audit;
            _actor = actor;
        }

        public async Task<PagedResult<EventViewModel>> GetPageAsync(int? pageNumber, CancellationToken cancellationToken = default)
        {
            var totalCount = await _events.CountAsync(cancellationToken);
            var pageRequest = new PageRequest(pageNumber);
            var normalizedPage = pageRequest.NormalizePageNumber(totalCount);
            var rows = await _events.GetPageAsync(
                pageRequest.GetOffset(normalizedPage),
                PageRequest.FixedPageSize,
                cancellationToken);
            var items = _mapper.Map<List<EventViewModel>>(rows);
            return new PagedResult<EventViewModel>(items, normalizedPage, totalCount);
        }

        public async Task<EventViewModel> GetEventAsync(int id, CancellationToken cancellationToken = default)
        {
            var eventItem = await _events.GetEventAsync(id, cancellationToken);
            return eventItem == null ? null : _mapper.Map<EventViewModel>(eventItem);
        }

        public async Task<EventFormOptions> GetFormOptionsAsync(CancellationToken cancellationToken = default)
        {
            var eventTypes = await _eventTypes.GetActiveEventTypesAsync(cancellationToken);
            var venues = await _venues.GetActiveVenuesAsync(cancellationToken);
            return new EventFormOptions
            {
                EventTypes = eventTypes.Select(eventType => new EventTypeOption(eventType.TypeId, eventType.TypeName)).ToList(),
                Venues = venues.Select(venue => new VenueOption(venue.VenueId, venue.RoomName, venue.Capacity)).ToList()
            };
        }

        public async Task<int> CreateDraftAsync(EventViewModel model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);
            var actorUserId = ActorUserId();
            var title = RequiredTitle(model);
            ValidateTimes(model);
            ValidateLimit(model);

            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var venue = await LockActiveReferencesAsync(model.TypeId, model.VenueId, ct);
                ValidateCapacity(model, venue);

                var eventItem = new Event
                {
                    Title = title,
                    Description = NormalizedDescription(model),
                    TypeId = model.TypeId,
                    VenueId = model.VenueId,
                    StartsAt = model.StartsAt,
                    RegistrationDeadline = model.RegistrationDeadline,
                    ParticipantLimit = model.ParticipantLimit,
                    EventStatusId = EventStatusCode.Draft,
                    CreatedBy = actorUserId,
                    CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
                };
                await _events.AddEventAsync(eventItem, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return eventItem.EventId;
            }, cancellationToken);
        }

        public async Task UpdateDraftAsync(EventViewModel model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);
            var actorUserId = ActorUserId();
            var title = RequiredTitle(model);
            ValidateTimes(model);
            ValidateLimit(model);

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var discovered = await _events.GetEventAsync(model.EventId, ct);
                if (discovered == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Event not found.");
                }

                var venue = await LockActiveReferencesAsync(model.TypeId, model.VenueId, ct);
                ValidateCapacity(model, venue);

                var eventItem = await _events.LockEventAsync(model.EventId, ct);
                if (eventItem == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Event not found.");
                }

                if (eventItem.EventStatusId != EventStatusCode.Draft)
                {
                    throw new DomainException(OutcomeCode.Invalid, "Only draft events can be edited.");
                }

                var oldValues = Snapshot(eventItem);
                eventItem.Title = title;
                eventItem.Description = NormalizedDescription(model);
                eventItem.TypeId = model.TypeId;
                eventItem.VenueId = model.VenueId;
                eventItem.StartsAt = model.StartsAt;
                eventItem.RegistrationDeadline = model.RegistrationDeadline;
                eventItem.ParticipantLimit = model.ParticipantLimit;
                eventItem.UpdatedBy = actorUserId;
                eventItem.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _events.UpdateEventAsync(eventItem, ct);
                _audit.Append(new AuditEntry(AuditActions.EventUpdated, "events", eventItem.EventId,
                    OldValues: oldValues, NewValues: Snapshot(eventItem)));
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        private sealed record EventAuditValues(
            string Title,
            string Description,
            int TypeId,
            string TypeName,
            int VenueId,
            string VenueName,
            DateTime StartsAt,
            DateTime RegistrationDeadline,
            int ParticipantLimit,
            EventStatusCode Status) : IAuditValues;

        // Names come from the read repositories so the audit records the old reference names too.
        private EventAuditValues Snapshot(Event eventItem) =>
            new(eventItem.Title,
                eventItem.Description,
                eventItem.TypeId,
                _eventTypes.GetEventType(eventItem.TypeId)?.TypeName,
                eventItem.VenueId,
                _venues.GetVenue(eventItem.VenueId)?.RoomName,
                eventItem.StartsAt,
                eventItem.RegistrationDeadline,
                eventItem.ParticipantLimit,
                eventItem.EventStatusId);

        private int ActorUserId()
        {
            if (!_actor.IsAuthenticated)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to change an event.");
            }

            int userId;
            try
            {
                userId = _actor.UserId;
            }
            catch (InvalidOperationException)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to change an event.");
            }

            if (userId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to change an event.");
            }

            return userId;
        }

        private static string RequiredTitle(EventViewModel model)
        {
            var title = model.Title?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException(OutcomeCode.Invalid, "Title is required.", nameof(EventViewModel.Title));
            }

            if (title.Length > 255)
            {
                throw new DomainException(OutcomeCode.Invalid, "Title cannot exceed 255 characters.", nameof(EventViewModel.Title));
            }

            return title;
        }

        private static string NormalizedDescription(EventViewModel model)
        {
            var description = model.Description?.Trim();
            return string.IsNullOrWhiteSpace(description) ? null : description;
        }

        private static void ValidateTimes(EventViewModel model)
        {
            if (model.StartsAt == default)
            {
                throw new DomainException(OutcomeCode.Invalid, "Start date and time is required.", nameof(EventViewModel.StartsAt));
            }

            if (model.RegistrationDeadline == default)
            {
                throw new DomainException(OutcomeCode.Invalid, "Signup deadline is required.", nameof(EventViewModel.RegistrationDeadline));
            }

            if (model.RegistrationDeadline > model.StartsAt)
            {
                throw new DomainException(OutcomeCode.Invalid, "The signup deadline cannot be after the event start.", nameof(EventViewModel.RegistrationDeadline));
            }
        }

        private static void ValidateLimit(EventViewModel model)
        {
            if (model.ParticipantLimit <= 0)
            {
                throw new DomainException(OutcomeCode.Invalid, "Participant limit must be greater than 0.", nameof(EventViewModel.ParticipantLimit));
            }
        }

        private static void ValidateCapacity(EventViewModel model, Venue venue)
        {
            if (model.ParticipantLimit > venue.Capacity)
            {
                throw new DomainException(OutcomeCode.Invalid, "Participant limit cannot exceed the venue capacity.", nameof(EventViewModel.ParticipantLimit));
            }
        }

        // AD-4: lock the Event Type, then the Venue, before the Event row is written.
        private async Task<Venue> LockActiveReferencesAsync(int typeId, int venueId, CancellationToken cancellationToken)
        {
            if (typeId <= 0)
            {
                throw new DomainException(OutcomeCode.Invalid, "Select an event type.", nameof(EventViewModel.TypeId));
            }

            if (venueId <= 0)
            {
                throw new DomainException(OutcomeCode.Invalid, "Select a venue.", nameof(EventViewModel.VenueId));
            }

            var eventType = await _eventTypes.LockEventTypeAsync(typeId, cancellationToken);
            if (eventType == null || !eventType.IsActive)
            {
                throw new DomainException(OutcomeCode.Invalid, "Select an active event type.", nameof(EventViewModel.TypeId));
            }

            var venue = await _venues.LockVenueAsync(venueId, cancellationToken);
            if (venue == null || !venue.IsActive)
            {
                throw new DomainException(OutcomeCode.Invalid, "Select an active venue.", nameof(EventViewModel.VenueId));
            }

            return venue;
        }
    }
}
