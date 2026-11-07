using System.Linq;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// The demo access overlay (register M-15b only; M-15a was removed by 5.6). Applied after the real
/// read in the controller, never as a repository decorator, because demo state is not a SQL row
/// (register rule 7). Only the membership status changes, and only for records the demo reactivation
/// command changed; a real deactivation clears the store entry, so the SQL row shows through.
/// </summary>
public sealed class DemoAccessStateOverlay : IAccessStateOverlay
{
    private readonly DemoStore _store;

    public DemoAccessStateOverlay(DemoStore store)
    {
        _store = store;
    }

    public MembershipAccessViewModel Apply(MembershipAccessViewModel model)
    {
        if (model == null)
        {
            return null;
        }

        if (_store.TryGetMembershipStatus(model.MemberId, out var status))
        {
            model.IsActive = status.Status == MembershipStatusCode.Active;
        }

        return model;
    }

    public PagedResult<MemberViewModel> Apply(PagedResult<MemberViewModel> page) =>
        page == null
            ? null
            : new PagedResult<MemberViewModel>(page.Items.Select(ApplyMember).ToList(), page.PageNumber, page.TotalCount);

    private MemberViewModel ApplyMember(MemberViewModel model)
    {
        if (model == null)
        {
            return null;
        }

        if (_store.TryGetMembershipStatus(model.MemberId, out var status))
        {
            model.Status = status.Status == MembershipStatusCode.Active ? "active" : "inactive";
        }

        return model;
    }
}
