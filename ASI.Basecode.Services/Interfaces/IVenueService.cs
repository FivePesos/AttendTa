using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IVenueService
    {
        List<VenueViewModel> GetVenues();
        VenueViewModel GetVenue(int id);
        Task AddVenueAsync(VenueViewModel model, int actorUserId, CancellationToken cancellationToken = default);
        Task UpdateVenueAsync(VenueViewModel model, int actorUserId, CancellationToken cancellationToken = default);
        Task ToggleActiveAsync(int id, int actorUserId, CancellationToken cancellationToken = default);
    }
}
