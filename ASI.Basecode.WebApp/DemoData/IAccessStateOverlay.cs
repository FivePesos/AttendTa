using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// Overlays demo membership status onto the existing member list read and the membership access
/// dialog model after the real read has run (register rule 7). Only the membership status changes,
/// and only for records a demo command changed. Nothing else from the real read is altered.
/// Account status has no overlay: its command writes SQL, so the real row is authoritative.
/// (Register M-15a, M-15b.)
/// </summary>
public interface IAccessStateOverlay
{
    PagedResult<MemberViewModel> Apply(PagedResult<MemberViewModel> page);

    MembershipAccessViewModel Apply(MembershipAccessViewModel model);
}
