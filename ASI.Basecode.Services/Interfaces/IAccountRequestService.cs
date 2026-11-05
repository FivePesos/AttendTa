using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Anonymous membership request submission.
    /// </summary>
    public interface IAccountRequestService
    {
        /// <summary>
        /// Validates an applicant's details, checks email and student-number uniqueness against
        /// accounts, member profiles and pending requests, and stores one pending request whose
        /// password is only an Argon2id hash.
        /// </summary>
        /// <param name="model">The submitted request details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that completes when the pending request is saved.</returns>
        /// <exception cref="ASI.Basecode.Resources.DomainException">
        /// Thrown with <c>Invalid</c> for field errors and <c>Duplicate</c> for an email or
        /// student number that is already in use.
        /// </exception>
        Task SubmitAsync(AccountRequestViewModel model, CancellationToken cancellationToken = default);
    }
}
