using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces;

public interface IAccountRequestRepository
{
    Task<AccountRequest> LockAccountRequestAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new account request. The caller saves through the unit of work.
    /// </summary>
    Task AddAsync(AccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a pending request already uses the email. Finalized requests are ignored.
    /// </summary>
    Task<bool> PendingEmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a pending request already uses the student number. Finalized requests are ignored.
    /// </summary>
    Task<bool> PendingStudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default);
}
