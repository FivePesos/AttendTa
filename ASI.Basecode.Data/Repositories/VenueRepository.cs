using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class VenueRepository : BaseRepository, IVenueRepository
    {
        public VenueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Venue> GetVenues()
        {
            return this.GetDbSet<Venue>();
        }

        public Venue GetVenue(int venueId)
        {
            return this.GetDbSet<Venue>().FirstOrDefault(v => v.VenueId == venueId);
        }

        public Task AddVenueAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            this.GetDbSet<Venue>().Add(venue);
            return Task.CompletedTask;
        }

        public Task UpdateVenueAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            this.SetEntityState(venue, EntityState.Modified);
            return Task.CompletedTask;
        }

        public bool VenueExists(int venueId)
        {
            return this.GetDbSet<Venue>().Any(v => v.VenueId == venueId);
        }
    }
}
