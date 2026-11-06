using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Data.Queries;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories;

public sealed class RegistrationRepository : BaseRepository, IRegistrationRepository
{
    public RegistrationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> IsConfirmedAsync(int registrationId, CancellationToken cancellationToken = default) =>
        GetDbSet<Registration>().AnyAsync(
            registration => registration.RegistrationId == registrationId
                && registration.RegistrationStatusId == RegistrationStatusCode.Confirmed,
            cancellationToken);

    public Task<bool> ExistsAsync(int eventId, int memberId, CancellationToken cancellationToken = default) =>
        GetDbSet<Registration>().AsNoTracking().AnyAsync(
            registration => registration.EventId == eventId && registration.MemberId == memberId,
            cancellationToken);

    public Task<int> CountConfirmedAsync(int eventId, CancellationToken cancellationToken = default) =>
        ParticipationFacts.SeatHolders(GetDbSet<Registration>().AsNoTracking())
            .CountAsync(registration => registration.EventId == eventId, cancellationToken);

    public Task AddAsync(Registration registration, CancellationToken cancellationToken = default)
    {
        GetDbSet<Registration>().Add(registration);
        return Task.CompletedTask;
    }
}
