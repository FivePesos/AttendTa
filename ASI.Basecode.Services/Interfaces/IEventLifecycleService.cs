using ASI.Basecode.Services.ServiceModels;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>Draft Event creation, draft editing, and the Management event list.</summary>
    public interface IEventLifecycleService
    {
        Task<PagedResult<EventViewModel>> GetPageAsync(int? pageNumber, CancellationToken cancellationToken = default);
        Task<EventViewModel> GetEventAsync(int id, CancellationToken cancellationToken = default);
        Task<EventFormOptions> GetFormOptionsAsync(CancellationToken cancellationToken = default);
        Task<int> CreateDraftAsync(EventViewModel model, CancellationToken cancellationToken = default);
        Task UpdateDraftAsync(EventViewModel model, CancellationToken cancellationToken = default);
    }
}
