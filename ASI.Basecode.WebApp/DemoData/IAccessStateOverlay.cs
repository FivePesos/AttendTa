using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// Overlays demo membership status onto the existing member list read and the membership access
/// dialog model after the real read has run (register rule 7). Only the membership status changes,
/// and only for records the demo reactivation command changed. Nothing else from the real read is
/// altered. Membership deactivation writes SQL, so its row is authoritative and its store entry is
/// cleared; account status has no overlay either. (Register M-15b; M-15a was removed by 5.6.)
/// </summary>
public interface IAccessStateOverlay
{
    PagedResult<MemberViewModel> Apply(PagedResult<MemberViewModel> page);

    MembershipAccessViewModel Apply(MembershipAccessViewModel model);
}
