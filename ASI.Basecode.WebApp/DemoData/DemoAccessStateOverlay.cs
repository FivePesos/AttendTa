using System.Linq;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// The demo access overlay (register M-15a and M-15b). Applied after the real read in the
/// controller, never as a repository decorator, because demo state is not a SQL row (register
/// rule 7). Only the membership status changes.
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
