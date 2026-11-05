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
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly IMapper _mapper;

        public VenueService(IVenueRepository repository, IUnitOfWork unitOfWork, IMapper mapper, TimeProvider timeProvider)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _mapper = mapper;
        }

        public List<VenueViewModel> GetVenues()
        {
            var venues = _repository.GetVenues().OrderBy(v => v.RoomName).ToList();
            return _mapper.Map<List<VenueViewModel>>(venues);
        }

        public VenueViewModel GetVenue(int id)
        {
            var venue = _repository.GetVenue(id);
            return venue == null ? null : _mapper.Map<VenueViewModel>(venue);
        }

        public async Task AddVenueAsync(VenueViewModel model, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to create a venue.");
            }

            if (string.IsNullOrWhiteSpace(model.ImagePath))
            {
                throw new DomainException(OutcomeCode.Invalid, "A venue photo is required.", nameof(VenueViewModel.ImagePath));
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var venue = new Venue();
                venue.RoomName = model.RoomName?.Trim();
                venue.Capacity = model.Capacity;
                venue.Location = model.Location;
                venue.AvailabilityNotes = model.AvailabilityNotes;
                venue.ContactPerson = model.ContactPerson;
                venue.ImagePath = model.ImagePath;
                venue.IsActive = true;
                venue.CreatedBy = actorUserId;
                venue.CreatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _repository.AddVenueAsync(venue, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task UpdateVenueAsync(VenueViewModel model, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update a venue.");
            }

            if (string.IsNullOrWhiteSpace(model.ImagePath))
            {
                throw new DomainException(OutcomeCode.Invalid, "A venue photo is required.", nameof(VenueViewModel.ImagePath));
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var venue = _repository.GetVenue(model.VenueId);
                if (venue == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Venue not found.");
                }

                venue.RoomName = model.RoomName?.Trim();
                venue.Capacity = model.Capacity;
                venue.Location = model.Location;
                venue.AvailabilityNotes = model.AvailabilityNotes;
                venue.ContactPerson = model.ContactPerson;
                venue.ImagePath = model.ImagePath;
                venue.UpdatedBy = actorUserId;
                venue.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _repository.UpdateVenueAsync(venue, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task ToggleActiveAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update a venue.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var venue = _repository.GetVenue(id);
                if (venue == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Venue not found.");
                }

                venue.IsActive = !venue.IsActive;
                venue.UpdatedBy = actorUserId;
                venue.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _repository.UpdateVenueAsync(venue, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }
    }
}
