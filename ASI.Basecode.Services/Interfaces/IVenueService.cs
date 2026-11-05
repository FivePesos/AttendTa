using ASI.Basecode.Services.ServiceModels;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IVenueService
    {
        Task<PagedResult<VenueViewModel>> GetPageAsync(int? pageNumber, CancellationToken cancellationToken = default);
        VenueViewModel GetVenue(int id);
        Task AddVenueAsync(VenueViewModel model, int actorUserId, Stream photo, string extension, CancellationToken cancellationToken = default);
        Task UpdateVenueAsync(VenueViewModel model, int actorUserId, Stream photo, string extension, CancellationToken cancellationToken = default);
        Task ToggleActiveAsync(int id, int actorUserId, CancellationToken cancellationToken = default);
    }
}
