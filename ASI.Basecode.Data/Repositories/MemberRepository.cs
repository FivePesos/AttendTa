using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class MemberRepository : BaseRepository, IMemberRepository
    {
        public MemberRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Member> GetMembers()
        {
            return this.GetDbSet<Member>();
        }

        public IQueryable<MemberRecord> GetMemberRecords()
        {
            var members = this.GetDbSet<Member>().AsNoTracking();
            var users = this.GetDbSet<User>().AsNoTracking();

            return from member in members
                   join user in users on member.UserId equals user.Id
                   select new MemberRecord
                   {
                       MemberId = member.MemberId,
                       UserId = member.UserId,
                       FullName = user.Name,
                       Email = user.UserId,
                       StudentNumber = member.StudentNumber,
                       Course = member.Course,
                       YearLevel = member.YearLevel,
                       Section = member.Section,
                       MobileNumber = member.MobileNumber,
                       MembershipType = member.MembershipType,
                       Status = member.MembershipStatusId == MembershipStatusCode.Active ? "active" : "inactive",
                       CreatedBy = member.CreatedBy,
                       CreatedAt = member.CreatedAt,
                       UpdatedBy = member.UpdatedBy,
                       UpdatedAt = member.UpdatedAt
                   };
        }

        public MemberRecord GetMemberRecord(int memberId)
        {
            return this.GetMemberRecords().FirstOrDefault(member => member.MemberId == memberId);
        }

        public Task<MemberRecord> GetMemberRecordByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return this.GetMemberRecords()
                .FirstOrDefaultAsync(member => member.UserId == userId, cancellationToken);
        }

        public Member GetMember(int memberId)
        {
            return this.GetDbSet<Member>().FirstOrDefault(m => m.MemberId == memberId);
        }

        public Task AddMemberAsync(Member member, CancellationToken cancellationToken = default)
        {
            this.GetDbSet<Member>().Add(member);
            return Task.CompletedTask;
        }

        public Task UpdateMemberAsync(Member member, CancellationToken cancellationToken = default)
        {
            if (Context.Entry(member).State == EntityState.Detached)
            {
                throw new System.InvalidOperationException("Only a tracked member can be updated.");
            }

            return Task.CompletedTask;
        }

        public bool UserHasMemberProfile(int userId)
        {
            return this.GetDbSet<Member>().Any(m => m.UserId == userId);
        }

        public bool StudentNumberExists(string studentNumber, int? excludeId = null)
        {
            return this.GetDbSet<Member>().Any(m => m.StudentNumber == studentNumber &&
                                                     (!excludeId.HasValue || m.MemberId != excludeId.Value));
        }
    }
}
