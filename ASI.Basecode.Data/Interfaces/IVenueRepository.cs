using ASI.Basecode.Data.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IVenueRepository
    {
        IQueryable<Venue> GetVenues();
        Venue GetVenue(int venueId);
        Task<Venue> LockVenueAsync(int venueId, CancellationToken cancellationToken = default);
        Task<bool> HasEventAboveCapacityAsync(int venueId, int capacity, CancellationToken cancellationToken = default);
        Task AddVenueAsync(Venue venue, CancellationToken cancellationToken = default);
        Task UpdateVenueAsync(Venue venue, CancellationToken cancellationToken = default);
        bool VenueExists(int venueId);
    }
}
