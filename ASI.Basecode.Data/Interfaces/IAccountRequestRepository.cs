using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces;

public interface IAccountRequestRepository
{
    Task<AccountRequest> LockAccountRequestAsync(int requestId, CancellationToken cancellationToken = default);
}
