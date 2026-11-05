using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources;
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
    public class EventTypeService : IEventTypeService
    {
        private readonly IEventTypeRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventTypeService(IEventTypeRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public List<EventTypeViewModel> GetEventTypes()
        {
            var eventTypes = _repository.GetEventTypes().OrderBy(t => t.TypeName).ToList();
            return _mapper.Map<List<EventTypeViewModel>>(eventTypes);
        }

        public EventTypeViewModel GetEventType(int id)
        {
            var eventType = _repository.GetEventType(id);
            return eventType == null ? null : _mapper.Map<EventTypeViewModel>(eventType);
        }

        public async Task AddEventTypeAsync(EventTypeViewModel model, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to create an event type.");
            }

            var normalizedTypeName = model.TypeName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedTypeName))
            {
                throw new DomainException(OutcomeCode.Invalid, "Type name is required.", nameof(EventTypeViewModel.TypeName));
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                throw new DomainException(OutcomeCode.Invalid, "Description is required.", nameof(EventTypeViewModel.Description));
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (_repository.TypeNameExists(normalizedTypeName))
                {
                    throw new DomainException(OutcomeCode.Duplicate, "Event type name already exists.", nameof(EventTypeViewModel.TypeName));
                }

                var eventType = new EventType();
                eventType.TypeName = normalizedTypeName;
                eventType.Description = model.Description;
                eventType.IsActive = true;
                eventType.CreatedBy = actorUserId;
                eventType.CreatedAt = DateTime.UtcNow;
                await _repository.AddEventTypeAsync(eventType, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task UpdateEventTypeAsync(EventTypeViewModel model, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update an event type.");
            }

            var normalizedTypeName = model.TypeName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedTypeName))
            {
                throw new DomainException(OutcomeCode.Invalid, "Type name is required.", nameof(EventTypeViewModel.TypeName));
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                throw new DomainException(OutcomeCode.Invalid, "Description is required.", nameof(EventTypeViewModel.Description));
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var eventType = _repository.GetEventType(model.TypeId);
                if (eventType == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Event type not found.");
                }

                if (_repository.TypeNameExists(normalizedTypeName, model.TypeId))
                {
                    throw new DomainException(OutcomeCode.Duplicate, "Event type name already exists.", nameof(EventTypeViewModel.TypeName));
                }

                eventType.TypeName = normalizedTypeName;
                eventType.Description = model.Description;
                eventType.UpdatedBy = actorUserId;
                eventType.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateEventTypeAsync(eventType, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task ToggleActiveAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update an event type.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var eventType = _repository.GetEventType(id);
                if (eventType == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Event type not found.");
                }

                eventType.IsActive = !eventType.IsActive;
                eventType.UpdatedBy = actorUserId;
                eventType.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateEventTypeAsync(eventType, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }
    }
}
