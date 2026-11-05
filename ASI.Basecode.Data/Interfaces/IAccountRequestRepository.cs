using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;

namespace ASI.Basecode.Data.Interfaces;

public interface IAccountRequestRepository
{
    Task<AccountRequest> LockAccountRequestAsync(int requestId, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(AccountRequestStatusCode status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountRequestQueueItem>> GetPageByStatusAsync(AccountRequestStatusCode status, int skip, int take, CancellationToken cancellationToken = default);
}
