using ASI.Basecode.Data.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IEventTypeRepository
    {
        IQueryable<EventType> GetEventTypes();
        EventType GetEventType(int typeId);
        Task AddEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default);
        Task UpdateEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default);
        bool EventTypeExists(int typeId);
        bool TypeNameExists(string typeName, int? excludeId = null);
    }
}
