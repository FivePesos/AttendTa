using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEventTypeService
    {
        List<EventTypeViewModel> GetEventTypes();
        EventTypeViewModel GetEventType(int id);
        Task AddEventTypeAsync(EventTypeViewModel model, int actorUserId, CancellationToken cancellationToken = default);
        Task UpdateEventTypeAsync(EventTypeViewModel model, int actorUserId, CancellationToken cancellationToken = default);
        Task ToggleActiveAsync(int id, int actorUserId, CancellationToken cancellationToken = default);
    }
}
