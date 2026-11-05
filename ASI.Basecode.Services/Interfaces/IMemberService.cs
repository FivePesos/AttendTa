using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IMemberService
    {
        List<MemberViewModel> GetMembers(string search = null);
        MemberViewModel GetMember(int id);
        Task<MemberViewModel> GetSelfProfileAsync(CancellationToken cancellationToken = default);
        Task UpdateMemberAsync(MemberViewModel model, int actorUserId, CancellationToken cancellationToken = default);
        Task ToggleStatusAsync(int id, int actorUserId, CancellationToken cancellationToken = default);
        Task CreateMemberFromApprovedAccountRequestAsync(int userId, MemberViewModel model, int actorUserId, CancellationToken cancellationToken = default);
    }
}
