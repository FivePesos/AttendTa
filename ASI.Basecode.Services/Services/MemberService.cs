using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ICurrentActor _currentActor;

        public MemberService(IMemberRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider, ICurrentActor currentActor)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _currentActor = currentActor;
        }

        public List<MemberViewModel> GetMembers(string search = null)
        {
            var query = _repository.GetMemberRecords();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.FullName.Contains(search) ||
                                         m.Email.Contains(search) ||
                                         m.StudentNumber.Contains(search));
            }

            return query.OrderBy(member => member.FullName)
                .ThenBy(member => member.MemberId)
                .Select(ToViewModel)
                .ToList();
        }

        public MemberViewModel GetMember(int id)
        {
            var member = _repository.GetMemberRecord(id);
            return member == null ? null : ToViewModel(member);
        }

        public async Task<MemberViewModel> GetSelfProfileAsync(CancellationToken cancellationToken = default)
        {
            if (!_currentActor.IsAuthenticated || !string.Equals(_currentActor.Role, RoleCodes.Member, StringComparison.Ordinal))
            {
                throw new DomainException(OutcomeCode.Forbidden, "Access denied.");
            }

            var member = await _repository.GetMemberRecordByUserIdAsync(_currentActor.UserId, cancellationToken);
            if (member == null)
            {
                throw new DomainException(OutcomeCode.NotFound, "Member profile not found.");
            }

            return ToViewModel(member);
        }

        public async Task CreateMemberFromApprovedAccountRequestAsync(int userId, MemberViewModel model, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid linked user and approving actor are required to create a member.");
            }

            if (userId <= 0)
            {
                throw new DomainException(OutcomeCode.Invalid, "A valid linked user and approving actor are required to create a member.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (_repository.UserHasMemberProfile(userId))
                {
                    throw new DomainException(OutcomeCode.Duplicate, "This user already has a member profile.");
                }

                if (_repository.StudentNumberExists(model.StudentNumber))
                {
                    throw new DomainException(OutcomeCode.Duplicate, "Student number already exists.", nameof(MemberViewModel.StudentNumber));
                }

                var member = new Member
                {
                    UserId = userId,
                    StudentNumber = model.StudentNumber,
                    Course = model.Course,
                    YearLevel = model.YearLevel,
                    Section = model.Section,
                    MobileNumber = model.MobileNumber,
                    MembershipType = model.MembershipType,
                    Status = "active",
                    CreatedBy = actorUserId,
                    CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
                };

                await _repository.AddMemberAsync(member, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task UpdateMemberAsync(MemberViewModel model, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update a member.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var member = _repository.GetMember(model.MemberId);
                if (member == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Member not found.");
                }

                if (_repository.StudentNumberExists(model.StudentNumber, model.MemberId))
                {
                    throw new DomainException(OutcomeCode.Duplicate, "Student number already exists.", nameof(MemberViewModel.StudentNumber));
                }

                member.StudentNumber = model.StudentNumber;
                member.Course = model.Course;
                member.YearLevel = model.YearLevel;
                member.Section = model.Section;
                member.MobileNumber = model.MobileNumber;
                member.MembershipType = model.MembershipType;
                member.Status = model.Status;
                member.UpdatedBy = actorUserId;
                member.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _repository.UpdateMemberAsync(member, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task ToggleStatusAsync(int id, int actorUserId, CancellationToken cancellationToken = default)
        {
            if (actorUserId <= 0)
            {
                throw new DomainException(OutcomeCode.Forbidden, "A valid authenticated user is required to update a member.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var member = _repository.GetMember(id);
                if (member == null)
                {
                    throw new DomainException(OutcomeCode.NotFound, "Member not found.");
                }

                member.Status = (member.Status ?? "active") == "active" ? "inactive" : "active";
                member.UpdatedBy = actorUserId;
                member.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _repository.UpdateMemberAsync(member, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        private static MemberViewModel ToViewModel(MemberRecord member)
        {
            return new MemberViewModel
            {
                MemberId = member.MemberId,
                UserId = member.UserId,
                FullName = member.FullName,
                Email = member.Email,
                StudentNumber = member.StudentNumber,
                Course = member.Course,
                YearLevel = member.YearLevel,
                Section = member.Section,
                MobileNumber = member.MobileNumber,
                MembershipType = member.MembershipType,
                Status = member.Status
            };
        }
    }
}
