using ASI.Basecode.Data.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IMemberRepository
    {
        IQueryable<MemberRecord> GetMemberRecords();
        MemberRecord GetMemberRecord(int memberId);
        Task<MemberRecord> GetMemberRecordByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Member GetMember(int memberId);
        Task AddMemberAsync(Member member, CancellationToken cancellationToken = default);
        Task UpdateMemberAsync(Member member, CancellationToken cancellationToken = default);
        bool UserHasMemberProfile(int userId);
        bool StudentNumberExists(string studentNumber, int? excludeId = null);
    }
}
