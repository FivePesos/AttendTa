using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Validates and stores anonymous membership requests. Only the Argon2id hash is persisted;
    /// the plaintext password and its confirmation never leave this call.
    /// </summary>
    public class AccountRequestService : IAccountRequestService
    {
        private readonly IAccountRequestRepository _accountRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TimeProvider _timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountRequestService"/> class.
        /// </summary>
        /// <param name="accountRequestRepository">The account request repository.</param>
        /// <param name="userRepository">The user repository (email uniqueness, including inactive accounts).</param>
        /// <param name="memberRepository">The member repository (student-number uniqueness, including inactive members).</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="passwordHasher">The Argon2id password hasher.</param>
        /// <param name="timeProvider">The clock used for <c>requested_at</c>.</param>
        public AccountRequestService(
            IAccountRequestRepository accountRequestRepository,
            IUserRepository userRepository,
            IMemberRepository memberRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            TimeProvider timeProvider)
        {
            _accountRequestRepository = accountRequestRepository ?? throw new ArgumentNullException(nameof(accountRequestRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        /// <inheritdoc />
        public async Task SubmitAsync(AccountRequestViewModel model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            Normalize(model);
            Validate(model);

            var email = model.Email;
            var studentNumber = model.StudentNumber;

            // Reuse the login lookup: it returns inactive accounts too, so a deactivated account
            // still owns its email address.
            if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            {
                throw new DomainException(OutcomeCode.Duplicate, "This email is already in use.", nameof(AccountRequestViewModel.Email));
            }

            // StudentNumberExists includes inactive member profiles.
            if (_memberRepository.StudentNumberExists(studentNumber))
            {
                throw new DomainException(OutcomeCode.Duplicate, "This student number is already in use.", nameof(AccountRequestViewModel.StudentNumber));
            }

            if (await _accountRequestRepository.PendingEmailExistsAsync(email, cancellationToken))
            {
                throw new DomainException(OutcomeCode.Duplicate, "An account request with this email is already pending.", nameof(AccountRequestViewModel.Email));
            }

            if (await _accountRequestRepository.PendingStudentNumberExistsAsync(studentNumber, cancellationToken))
            {
                throw new DomainException(OutcomeCode.Duplicate, "An account request with this student number is already pending.", nameof(AccountRequestViewModel.StudentNumber));
            }

            var request = new AccountRequest
            {
                StudentNumber = studentNumber,
                FullName = model.FullName,
                Email = email,
                PasswordHash = _passwordHasher.Hash(model.Password),
                Course = model.Course,
                YearLevel = model.YearLevel.Value,
                Section = model.Section,
                MobileNumber = model.MobileNumber,
                AccountRequestStatusId = AccountRequestStatusCode.Pending,
                RequestedAt = _timeProvider.GetUtcNow().UtcDateTime
            };

            // One insert, no explicit transaction. The pending-only filtered unique indexes are
            // the final guard; the unit of work translates a race loser's SQL 2601/2627 into
            // DomainException(Duplicate) without a field.
            await _accountRequestRepository.AddAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static void Normalize(AccountRequestViewModel model)
        {
            model.StudentNumber = model.StudentNumber?.Trim();
            model.FullName = model.FullName?.Trim();
            model.Email = model.Email?.Trim();
            model.Course = model.Course?.Trim();
            model.Section = model.Section?.Trim();
            model.MobileNumber = string.IsNullOrWhiteSpace(model.MobileNumber) ? null : model.MobileNumber.Trim();
        }

        private static void Validate(AccountRequestViewModel model)
        {
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
            {
                return;
            }

            var first = results[0];
            var field = first.MemberNames.FirstOrDefault();
            throw new DomainException(OutcomeCode.Invalid, first.ErrorMessage ?? "The request contains invalid details.", field);
        }
    }
}
