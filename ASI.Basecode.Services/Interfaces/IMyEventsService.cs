using ASI.Basecode.Services.ServiceModels;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces;

/// <summary>
/// The member Participation History reads. The demo provider composes the existing per-event reads
/// now (register M-02 and M-08); 3.13 and 4.7 implement the real owner-scoped queries behind these
/// same contracts, so the controller and views do not change.
/// </summary>
public interface IMyEventsService
{
    /// <summary>
    /// Returns the actor's own registrations across events, newest start first, one shared page
    /// of at most <see cref="PageRequest.FixedPageSize"/> cards, with the status label and badge
    /// already resolved for the current time.
    /// </summary>
    Task<PagedResult<MyEventRegistrationCardViewModel>> GetRegistrationsAsync(
        int? pageNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the actor's own attendance records, newest recorded first, one shared page of at
    /// most <see cref="PageRequest.FixedPageSize"/> rows, with the method label and the
    /// invalidated state already resolved.
    /// </summary>
    Task<PagedResult<MyEventAttendanceRowViewModel>> GetAttendanceAsync(
        int? pageNumber,
        CancellationToken cancellationToken = default);
}
