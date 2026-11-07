using System;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// Demo membership access commands (register M-15b only; M-15a was removed by 5.6). Deactivation is
/// the real <see cref="MembershipAccessService"/> command, so it writes SQL, cancels the member's
/// upcoming registrations and appends the audit row; the store entry is then cleared so no overlay
/// masks the authoritative row. Reactivation stays demo-only: it reloads the real member, applies
/// the same refusal rules 5.7 will, and changes only the in-memory store (register rules 4 and 8).
/// Account access is never touched.
/// </summary>
public sealed class DemoMembershipAccessCommands : IMembershipAccessCommands
{
    private readonly IMemberRepository _members;
    private readonly DemoStore _store;
    private readonly TimeProvider _timeProvider;
    private readonly MembershipAccessService _membershipAccessService;

    public DemoMembershipAccessCommands(
        IMemberRepository members,
        DemoStore store,
        TimeProvider timeProvider,
        MembershipAccessService membershipAccessService)
    {
        _members = members;
        _store = store;
        _timeProvider = timeProvider;
        _membershipAccessService = membershipAccessService;
    }

    public async Task DeactivateAsync(int memberId, string reason, CancellationToken cancellationToken = default)
    {
        await _membershipAccessService.DeactivateAsync(memberId, reason, cancellationToken);
        _store.ClearMembershipStatus(memberId);
    }

    public Task ReactivateAsync(int memberId, string reason, CancellationToken cancellationToken = default) =>
        SetStatusAsync(memberId, MembershipStatusCode.Active, reason, cancellationToken);

    private Task SetStatusAsync(int memberId, MembershipStatusCode status, string reason, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Refuse a blank reason before the state check, so the reason refusal wins over the
        // already-active and already-inactive refusals.
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException(OutcomeCode.Invalid, "A reason is required.", "reason");
        }

        var member = _members.GetMemberRecord(memberId);
        if (member is null)
        {
            throw new DomainException(OutcomeCode.NotFound, "Member not found.");
        }

        // The state the screen shows: the real row, overlaid by the store when a demo command already
        // changed it. The store never writes SQL, so the real row alone would refuse the second step.
        var isActive = _store.TryGetMembershipStatus(memberId, out var stored)
            ? stored.Status == MembershipStatusCode.Active
            : string.Equals(member.Status, "active", StringComparison.OrdinalIgnoreCase);

        if (isActive == (status == MembershipStatusCode.Active))
        {
            throw new DomainException(
                OutcomeCode.Invalid,
                status == MembershipStatusCode.Active
                    ? "This membership is already active."
                    : "This membership is already inactive.");
        }

        _store.SetMembershipStatus(memberId, status, reason, _timeProvider.GetUtcNow().UtcDateTime);
        return Task.CompletedTask;
    }
}
