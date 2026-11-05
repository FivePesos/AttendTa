using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces;

public interface IAccountAdministrationRepository
{
    /// <summary>
    /// Counts accounts whose full name or email contains the trimmed search term.
    /// </summary>
    Task<int> CountAsync(string search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads one page of safe account summaries ordered by full name, then account ID.
    /// </summary>
    Task<IReadOnlyList<AccountListItem>> GetPageAsync(string search, int skip, int take, CancellationToken cancellationToken = default);
}
