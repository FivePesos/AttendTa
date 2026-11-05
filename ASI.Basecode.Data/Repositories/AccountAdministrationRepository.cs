using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories;

public sealed class AccountAdministrationRepository(IUnitOfWork unitOfWork) : BaseRepository(unitOfWork), IAccountAdministrationRepository
{
    public Task<int> CountAsync(string search, CancellationToken cancellationToken = default) =>
        Filter(search).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<AccountListItem>> GetPageAsync(
        string search,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        return await (from user in Filter(search)
                      join role in GetDbSet<RoleLookup>().AsNoTracking() on user.RoleId equals role.RoleId
                      orderby user.Name, user.Id
                      select new AccountListItem
                      {
                          AccountId = user.Id,
                          FullName = user.Name,
                          Email = user.UserId,
                          RoleCode = role.Code,
                          RoleLabel = role.Label,
                          IsActive = user.IsActive
                      })
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<User> Filter(string search)
    {
        var query = GetDbSet<User>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user => user.Name.Contains(term) || user.UserId.Contains(term));
        }

        return query;
    }
}
