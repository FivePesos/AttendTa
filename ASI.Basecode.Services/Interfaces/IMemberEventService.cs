using ASI.Basecode.Services.ServiceModels;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Participant-facing published event browsing and registration submission. Any authenticated
    /// role may browse; registration additionally requires an eligible member profile.
    /// </summary>
    public interface IMemberEventService
    {
        Task<PagedResult<MemberEventListItemViewModel>> GetFeedAsync(int? pageNumber, CancellationToken cancellationToken = default);

        /// <summary>Returns <c>null</c> for an event that does not exist or is not published.</summary>
        Task<MemberEventDetailViewModel> GetDetailAsync(int eventId, CancellationToken cancellationToken = default);

        Task<MemberRegistrationResult> RegisterAsync(int eventId, CancellationToken cancellationToken = default);
    }
}
