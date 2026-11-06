using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Data.Queries;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

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

    private static bool Open(RegistrationStatusCode status) => status is RegistrationStatusCode.Pending or RegistrationStatusCode.Confirmed or RegistrationStatusCode.Waitlisted;

    public async Task<Registration> GetForCancellationAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var snapshot = await GetDbSet<Registration>().AsNoTracking().SingleOrDefaultAsync(r => r.RegistrationId == registrationId, cancellationToken);
        if (snapshot == null) return null;
        var tracked = GetDbSet<Registration>().Local.FirstOrDefault(r => r.RegistrationId == registrationId);
        if (tracked == null) { GetDbSet<Registration>().Attach(snapshot); return snapshot; }
        var entry = Context.Entry(tracked);
        Context.ChangeTracker.DetectChanges();
        var fresh = Context.Entry(snapshot).CurrentValues;
        foreach (var property in entry.Properties)
        {
            var value = fresh[property.Metadata.Name];
            if (!property.IsModified) property.CurrentValue = value;
            property.OriginalValue = value;
        }
        // The persisted status is authoritative for idempotence, even if a caller tracked it earlier.
        entry.Property(r => r.RegistrationStatusId).CurrentValue = snapshot.RegistrationStatusId;
        entry.Property(r => r.RegistrationStatusId).IsModified = false;
        return tracked;
    }

    public async Task<IReadOnlyList<Registration>> GetOpenForEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var ids = await GetDbSet<Registration>().AsNoTracking().Where(r => r.EventId == eventId && r.RegistrationStatusId != RegistrationStatusCode.Cancelled).OrderBy(r => r.RegistrationId).Select(r => r.RegistrationId).ToListAsync(cancellationToken);
        var result = new List<Registration>();
        foreach (var id in ids) { var row = await GetForCancellationAsync(id, cancellationToken); if (row != null && Open(row.RegistrationStatusId)) result.Add(row); }
        return result;
    }

    public async Task<IReadOnlyList<int>> DiscoverOpenEventIdsForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await (from r in GetDbSet<Registration>().AsNoTracking()
               join m in GetDbSet<Member>().AsNoTracking() on r.MemberId equals m.MemberId
               where m.UserId == userId && r.RegistrationStatusId != RegistrationStatusCode.Cancelled
               select r.EventId).Distinct().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UpcomingRegistrationRow>> GetUpcomingForUserAsync(int userId, DateTime nowUtc, int? eventId = null, CancellationToken cancellationToken = default)
    {
        var validAttendance = ParticipationFacts.ValidAttendances(GetDbSet<Attendance>().AsNoTracking());
        var rows = await (from r in GetDbSet<Registration>().AsNoTracking()
                          join m in GetDbSet<Member>().AsNoTracking() on r.MemberId equals m.MemberId
                          join e in GetDbSet<Event>().AsNoTracking() on r.EventId equals e.EventId
                          where m.UserId == userId && (!eventId.HasValue || e.EventId == eventId.Value)
                            && (r.RegistrationStatusId == RegistrationStatusCode.Pending || r.RegistrationStatusId == RegistrationStatusCode.Confirmed || r.RegistrationStatusId == RegistrationStatusCode.Waitlisted)
                            && !validAttendance.Any(a => a.RegistrationId == r.RegistrationId)
                          select new UpcomingRegistrationRow(r.RegistrationId, e.EventId, e.Title, e.StartsAt)).ToListAsync(cancellationToken);
        // Compare in memory: SQL datetime2(3) parameter coercion must not round away a tick.
        var cutoff = ParticipationFacts.StartCutoffPassed(nowUtc).Compile();
        return rows.Where(r => !cutoff(new Event { StartsAt = r.StartsAt })).OrderBy(r => r.StartsAt).ThenBy(r => r.EventId).ThenBy(r => r.RegistrationId).ToList();
    }
}
