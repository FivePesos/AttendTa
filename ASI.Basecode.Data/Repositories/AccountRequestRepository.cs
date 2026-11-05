using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories;

public sealed class AccountRequestRepository(IUnitOfWork unitOfWork) : BaseRepository(unitOfWork), IAccountRequestRepository
{
    public async Task<AccountRequest> LockAccountRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null) throw new InvalidOperationException("A transaction is required to lock an account request.");
        var row = await GetDbSet<AccountRequest>().FromSqlInterpolated($"SELECT * FROM dbo.account_requests WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE request_id = {requestId}")
            .AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        var tracked = GetDbSet<AccountRequest>().Local.FirstOrDefault(x => x.RequestId == requestId);
        if (row == null) { if (tracked != null) Context.Entry(tracked).State = EntityState.Detached; return null; }
        if (tracked == null) { GetDbSet<AccountRequest>().Attach(row); return row; }
        Context.Entry(tracked).CurrentValues.SetValues(row);
        Context.Entry(tracked).OriginalValues.SetValues(row);
        Context.Entry(tracked).State = EntityState.Unchanged;
        return tracked;
    }

    public Task AddAsync(AccountRequest request, CancellationToken cancellationToken = default)
    {
        GetDbSet<AccountRequest>().Add(request);
        return Task.CompletedTask;
    }

    public Task<bool> PendingEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult(false);
        }

        return GetDbSet<AccountRequest>()
            .AsNoTracking()
            .AnyAsync(
                request => request.AccountRequestStatusId == AccountRequestStatusCode.Pending
                    && request.Email == email,
                cancellationToken);
    }

    public Task<bool> PendingStudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return Task.FromResult(false);
        }

        return GetDbSet<AccountRequest>()
            .AsNoTracking()
            .AnyAsync(
                request => request.AccountRequestStatusId == AccountRequestStatusCode.Pending
                    && request.StudentNumber == studentNumber,
                cancellationToken);
    }
}
