using System;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// Demo account access commands (register M-14a and M-14b). Every command reloads the real account
/// and applies the same refusal rules 5.4 and 5.5 will, then changes only the in-memory store: no
/// SQL row, no stored file and no Activity Log entry (register rules 4 and 8).
/// </summary>
public sealed class DemoAccountAccessCommands : IAccountAccessCommands
{
    private readonly IAccountAdministrationRepository _accounts;
    private readonly DemoStore _store;
    private readonly TimeProvider _timeProvider;

    public DemoAccountAccessCommands(
        IAccountAdministrationRepository accounts,
        DemoStore store,
        TimeProvider timeProvider)
    {
        _accounts = accounts;
        _store = store;
        _timeProvider = timeProvider;
    }

    public async Task DeactivateAsync(int accountId, string reason, CancellationToken cancellationToken = default)
    {
        RequireReason(reason);
        var account = await RequireAccountAsync(accountId, cancellationToken);
        if (!IsActive(account))
        {
            throw new DomainException(OutcomeCode.Invalid, "This account is already inactive.");
        }

        if (string.Equals(account.RoleCode, RoleCodes.Admin, StringComparison.Ordinal)
            && await _accounts.CountActiveAdministratorsAsync(cancellationToken) <= 1)
        {
            throw new DomainException(OutcomeCode.Invalid, "The last active Administrator cannot be deactivated.");
        }

        _store.SetAccountActive(account.AccountId, false, reason, _timeProvider.GetUtcNow().UtcDateTime);
    }

    public async Task ReactivateAsync(int accountId, string reason, CancellationToken cancellationToken = default)
    {
        RequireReason(reason);
        var account = await RequireAccountAsync(accountId, cancellationToken);
        if (IsActive(account))
        {
            throw new DomainException(OutcomeCode.Invalid, "This account is already active.");
        }

        _store.SetAccountActive(account.AccountId, true, reason, _timeProvider.GetUtcNow().UtcDateTime);
    }

    /// <summary>
    /// Refuses a blank reason before any state check, so the reason refusal wins over the
    /// already-inactive and last-administrator refusals.
    /// </summary>
    private static void RequireReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException(OutcomeCode.Invalid, "A reason is required.", "reason");
        }
    }

    /// <summary>
    /// The state the screen shows: the real row, overlaid by the store when a demo command already
    /// changed it. The store never writes SQL, so the real row alone would refuse the second step.
    /// </summary>
    private bool IsActive(AccountListItem account) =>
        _store.TryGetAccountStatus(account.AccountId, out var status) ? status.IsActive : account.IsActive;

    private async Task<AccountListItem> RequireAccountAsync(int accountId, CancellationToken cancellationToken)
    {
        var account = await _accounts.GetAccountAsync(accountId, cancellationToken);
        if (account is null)
        {
            throw new DomainException(OutcomeCode.NotFound, "Account not found.");
        }

        return account;
    }
}
