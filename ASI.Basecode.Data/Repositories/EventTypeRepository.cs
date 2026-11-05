using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class EventTypeRepository : BaseRepository, IEventTypeRepository
    {
        public EventTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<EventType> GetEventTypes()
        {
            return this.GetDbSet<EventType>();
        }

        public EventType GetEventType(int typeId)
        {
            return this.GetDbSet<EventType>().FirstOrDefault(t => t.TypeId == typeId);
        }

        public Task AddEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default)
        {
            this.GetDbSet<EventType>().Add(eventType);
            return Task.CompletedTask;
        }

        public Task UpdateEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default)
        {
            this.SetEntityState(eventType, EntityState.Modified);
            return Task.CompletedTask;
        }

        public bool EventTypeExists(int typeId)
        {
            return this.GetDbSet<EventType>().Any(t => t.TypeId == typeId);
        }

        public bool TypeNameExists(string typeName, int? excludeId = null)
        {
            return this.GetDbSet<EventType>().Any(t => t.TypeName == typeName &&
                                                        (!excludeId.HasValue || t.TypeId != excludeId.Value));
        }
    }
}
