using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces;

/// <summary>
/// Membership access commands for the Management members list. Deactivation is the real
/// transactional command (5.6) and raises the same <see cref="ASI.Basecode.Resources.DomainException"/>
/// outcomes as the screen expects; reactivation stays demo-only until 5.7. Membership status is
/// separate from account access.
/// </summary>
public interface IMembershipAccessCommands
{
    /// <summary>Deactivates a membership. A reason is required.</summary>
    Task DeactivateAsync(int memberId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Reactivates a membership. A reason is required.</summary>
    Task ReactivateAsync(int memberId, string reason, CancellationToken cancellationToken = default);
}
