using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Data.Queries;
using ASI.Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

/// <summary>
/// Focused SQL smoke for the report's one snapshot read: every registration of the event comes back
/// with its member, signup snapshots and attendance state, including inactive history, and the rows
/// order by participant name then registration id with stable boundaries. The summary, no-shows and
/// distribution are derived from this snapshot by the service.
/// </summary>
public sealed class ParticipationReportRepositoryTests(SqlServerDatabaseFixture fixture) : IClassFixture<SqlServerDatabaseFixture>
{
    [Fact]
    public async Task SnapshotReturnsEveryRegistrationWithAttendanceStateIncludingInactiveHistory()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
        var suffix = Guid.NewGuid().ToString("N");

        await using var context = Context();
        var actor = new User
        {
            UserId = $"report-actor-{suffix}@attendta.test",
            Name = "Report Actor",
            Password = "test",
            RoleId = RoleCode.Officer,
            IsActive = true
        };
        context.Users.Add(actor);
        await context.SaveChangesAsync(ct);

        var eventType = new EventType
        {
            TypeName = $"Report type {suffix}",
            Description = "Report test type",
            IsActive = true,
            CreatedBy = actor.Id,
            CreatedAt = now
        };
        var venue = new Venue
        {
            RoomName = $"Report room {suffix}",
            Capacity = 50,
            ImagePath = "uploads/venues/test.jpg",
            IsActive = true,
            CreatedBy = actor.Id,
            CreatedAt = now
        };
        context.EventTypes.Add(eventType);
        context.Venues.Add(venue);
        await context.SaveChangesAsync(ct);

        var completed = EventItem(completed: true, now, eventType.TypeId, venue.VenueId, actor.Id, suffix);
        var cancelled = EventItem(completed: false, now, eventType.TypeId, venue.VenueId, actor.Id, suffix);
        context.Events.AddRange(completed, cancelled);
        await context.SaveChangesAsync(ct);

        // One member per registration: the schema allows one registration per member/event.
        var activeMembers = new List<Member>();
        for (var index = 1; index <= 6; index++)
        {
            activeMembers.Add(await SeedMemberAsync($"report-active-{index}-{suffix}@attendta.test", isActive: true, MembershipStatusCode.Active, actor.Id, now, ct));
        }

        var inactiveMember = await SeedMemberAsync($"report-inactive-{suffix}@attendta.test", isActive: false, MembershipStatusCode.Inactive, actor.Id, now, ct);

        // Completed event: one valid attendee, one invalidated check-in, one never checked in, plus
        // pending, waitlisted and cancelled rows.
        var attended = Registration(completed.EventId, activeMembers[0].MemberId, RegistrationStatusCode.Confirmed, "BSIT", 1, actor.Id, now);
        var invalidated = Registration(completed.EventId, activeMembers[1].MemberId, RegistrationStatusCode.Confirmed, "BSED", 2, actor.Id, now);
        var neverCheckedIn = Registration(completed.EventId, activeMembers[2].MemberId, RegistrationStatusCode.Confirmed, "BSIT", 3, actor.Id, now);
        var pending = Registration(completed.EventId, activeMembers[3].MemberId, RegistrationStatusCode.Pending, "BSIT", 1, actor.Id, now);
        var waitlisted = Registration(completed.EventId, activeMembers[4].MemberId, RegistrationStatusCode.Waitlisted, "BSED", 2, actor.Id, now);
        var cancelledRow = Registration(completed.EventId, activeMembers[5].MemberId, RegistrationStatusCode.Cancelled, "BSIT", 4, actor.Id, now);
        // The inactive member still attended, so the snapshot must keep them.
        var inactiveAttended = Registration(completed.EventId, inactiveMember.MemberId, RegistrationStatusCode.Confirmed, "BSED", 2, actor.Id, now);
        context.Registrations.AddRange(attended, invalidated, neverCheckedIn, pending, waitlisted, cancelledRow, inactiveAttended);
        await context.SaveChangesAsync(ct);

        context.AttendanceRecords.AddRange(
            Attendance(attended.RegistrationId, actor.Id, now),
            Attendance(invalidated.RegistrationId, actor.Id, now, invalidatedAt: now),
            Attendance(inactiveAttended.RegistrationId, actor.Id, now));
        await context.SaveChangesAsync(ct);

        // Cancelled event: a confirmed row with and without valid attendance.
        var cancelledAttended = Registration(cancelled.EventId, activeMembers[0].MemberId, RegistrationStatusCode.Confirmed, "BSIT", 1, actor.Id, now);
        var cancelledNoShow = Registration(cancelled.EventId, inactiveMember.MemberId, RegistrationStatusCode.Confirmed, "BSED", 2, actor.Id, now);
        context.Registrations.AddRange(cancelledAttended, cancelledNoShow);
        await context.SaveChangesAsync(ct);
        context.AttendanceRecords.Add(Attendance(cancelledAttended.RegistrationId, actor.Id, now));
        await context.SaveChangesAsync(ct);

        var repository = new ParticipationReportRepository(new UnitOfWork(context));

        var snapshot = await repository.GetSnapshotAsync(completed.EventId, ct);

        // Every registration of the event is present, in every status.
        Assert.Equal(7, snapshot.Count);
        Assert.Equal(4, snapshot.Count(row => ParticipationFacts.HoldsSeat(row.Status)));
        Assert.Equal(1, snapshot.Count(row => row.Status == RegistrationStatusCode.Pending));
        Assert.Equal(1, snapshot.Count(row => row.Status == RegistrationStatusCode.Waitlisted));
        Assert.Equal(1, snapshot.Count(row => row.Status == RegistrationStatusCode.Cancelled));

        // The invalidated check-in keeps its recorded time and is flagged by the invalidation time.
        var invalidatedRow = snapshot.Single(row => row.RegistrationId == invalidated.RegistrationId);
        Assert.NotNull(invalidatedRow.CheckedInAt);
        Assert.NotNull(invalidatedRow.InvalidatedAt);
        Assert.True(invalidatedRow.IsAttendanceInvalidated);

        // A never-checked-in row has no attendance facts at all.
        var absentRow = snapshot.Single(row => row.RegistrationId == neverCheckedIn.RegistrationId);
        Assert.Null(absentRow.CheckedInAt);
        Assert.Null(absentRow.InvalidatedAt);
        Assert.False(absentRow.IsAttendanceInvalidated);

        // The inactive member's valid attendance is still in the snapshot: the read never joins the
        // active-account or active-membership flags.
        var inactiveRow = snapshot.Single(row => row.RegistrationId == inactiveAttended.RegistrationId);
        Assert.NotNull(inactiveRow.CheckedInAt);
        Assert.Null(inactiveRow.InvalidatedAt);

        // The snapshot carries the signup snapshots and the linked member's student number.
        Assert.Equal("BSED", invalidatedRow.Course);
        Assert.Equal(2, invalidatedRow.YearLevel);
        Assert.Equal(activeMembers[1].StudentNumber, invalidatedRow.StudentNumber);

        // The derived facts agree with the raw rows: two valid attendees, four confirmed seats and
        // therefore two no-shows on the completed event.
        var validAttendees = snapshot.Count(row =>
            ParticipationFacts.HoldsSeat(row.Status)
            && row.CheckedInAt is not null
            && ParticipationFacts.IsValidAttendance(row.InvalidatedAt));
        Assert.Equal(2, validAttendees);
        Assert.Equal(2, snapshot.Count(row => ParticipationFacts.HoldsSeat(row.Status)) - validAttendees);
    }

    [Fact]
    public async Task SnapshotOrdersByNameThenRegistrationIdWithStableBoundaries()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
        var suffix = Guid.NewGuid().ToString("N");

        await using var context = Context();
        var actor = new User
        {
            UserId = $"report-page-actor-{suffix}@attendta.test",
            Name = "Report Actor",
            Password = "test",
            RoleId = RoleCode.Officer,
            IsActive = true
        };
        context.Users.Add(actor);
        await context.SaveChangesAsync(ct);

        var eventType = new EventType
        {
            TypeName = $"Report page type {suffix}",
            Description = "Report test type",
            IsActive = true,
            CreatedBy = actor.Id,
            CreatedAt = now
        };
        var venue = new Venue
        {
            RoomName = $"Report page room {suffix}",
            Capacity = 50,
            ImagePath = "uploads/venues/test.jpg",
            IsActive = true,
            CreatedBy = actor.Id,
            CreatedAt = now
        };
        context.EventTypes.Add(eventType);
        context.Venues.Add(venue);
        await context.SaveChangesAsync(ct);

        var completed = new Event
        {
            Title = $"Report paged {suffix}",
            Description = "Report test event",
            TypeId = eventType.TypeId,
            VenueId = venue.VenueId,
            StartsAt = now,
            RegistrationDeadline = now.AddHours(-1),
            ParticipantLimit = 40,
            EventStatusId = EventStatusCode.Completed,
            ImagePath = "uploads/events/11111111111111111111111111111111.jpg",
            CreatedBy = actor.Id,
            CreatedAt = now
        };
        context.Events.Add(completed);
        await context.SaveChangesAsync(ct);

        // Names descend as registration ids ascend, so a page ordered by name is not ordered by id.
        // The two lowest ids share the largest name, so the tie breaks on registration_id at the
        // boundary between page 1 and page 2.
        var members = new List<Member>();
        for (var index = 1; index <= 26; index++)
        {
            var name = index <= 2 ? "Participant 26" : $"Participant {27 - index:D2}";
            members.Add(await SeedMemberAsync(
                $"report-page-{index}-{suffix}@attendta.test",
                isActive: true,
                MembershipStatusCode.Active,
                actor.Id,
                now,
                ct,
                name));
        }

        var registrations = new List<Registration>();
        for (var index = 0; index < members.Count; index++)
        {
            registrations.Add(Registration(
                completed.EventId,
                members[index].MemberId,
                RegistrationStatusCode.Confirmed,
                index == 1 ? "BSED" : "BSIT",
                1 + (index % 4),
                actor.Id,
                now));
        }

        context.Registrations.AddRange(registrations);
        await context.SaveChangesAsync(ct);

        // One valid check-in and one invalidated check-in inside the ordered set; the rest are absent.
        context.AttendanceRecords.AddRange(
            Attendance(registrations[2].RegistrationId, actor.Id, now),
            Attendance(registrations[1].RegistrationId, actor.Id, now, invalidatedAt: now));
        await context.SaveChangesAsync(ct);

        var repository = new ParticipationReportRepository(new UnitOfWork(context));

        var snapshot = await repository.GetSnapshotAsync(completed.EventId, ct);

        Assert.Equal(26, snapshot.Count);

        var first = snapshot.Take(25).ToList();
        var lastRow = Assert.Single(snapshot.Skip(25));

        // Names ascend across the boundary; the shared name breaks on the lower id first.
        Assert.Equal("Participant 01", first[0].MemberName);
        Assert.Equal("Participant 24", first[23].MemberName);
        Assert.Equal("Participant 26", first[24].MemberName);
        Assert.Equal(registrations[0].RegistrationId, first[24].RegistrationId);
        Assert.Equal("Participant 26", lastRow.MemberName);
        Assert.Equal(registrations[1].RegistrationId, lastRow.RegistrationId);
        Assert.True(lastRow.RegistrationId > first[24].RegistrationId);

        // No row is repeated or skipped.
        Assert.Equal(26, snapshot.Select(row => row.RegistrationId).Distinct().Count());

        // The snapshot course and the linked member's student number come back on the row.
        Assert.Equal("BSED", lastRow.Course);
        Assert.Equal(members[1].StudentNumber, lastRow.StudentNumber);
        Assert.Equal(2, lastRow.YearLevel);

        // The invalidated row is flagged, a valid row is not, and an absent row has no check-in time.
        var invalidatedRow = snapshot.Single(row => row.RegistrationId == registrations[1].RegistrationId);
        Assert.True(invalidatedRow.IsAttendanceInvalidated);
        Assert.NotNull(invalidatedRow.CheckedInAt);
        var validRow = snapshot.Single(row => row.RegistrationId == registrations[2].RegistrationId);
        Assert.False(validRow.IsAttendanceInvalidated);
        Assert.NotNull(validRow.CheckedInAt);
        var absentRow = snapshot.Single(row => row.RegistrationId == registrations[5].RegistrationId);
        Assert.Null(absentRow.CheckedInAt);
        Assert.False(absentRow.IsAttendanceInvalidated);
    }

    private AsiBasecodeDBContext Context() => new(
        new DbContextOptionsBuilder<AsiBasecodeDBContext>().UseSqlServer(fixture.ConnectionString).Options);

    private async Task<Member> SeedMemberAsync(
        string email,
        bool isActive,
        MembershipStatusCode status,
        int actorId,
        DateTime now,
        CancellationToken cancellationToken,
        string name = "Report Member")
    {
        await using var context = Context();
        var user = new User
        {
            UserId = email,
            Name = name,
            Password = "test",
            RoleId = RoleCode.Member,
            IsActive = isActive
        };
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        var member = new Member
        {
            UserId = user.Id,
            StudentNumber = $"RPT-{Guid.NewGuid():N}",
            Course = "BSIT",
            YearLevel = 1,
            Section = "A",
            MembershipStatusId = status,
            CreatedBy = actorId,
            CreatedAt = now
        };
        context.Members.Add(member);
        await context.SaveChangesAsync(cancellationToken);
        return member;
    }

    private static Event EventItem(bool completed, DateTime now, int typeId, int venueId, int actorId, string suffix) => new()
    {
        Title = $"Report {(completed ? "completed" : "cancelled")} {suffix}",
        Description = "Report test event",
        TypeId = typeId,
        VenueId = venueId,
        StartsAt = now,
        RegistrationDeadline = now.AddHours(-1),
        ParticipantLimit = 20,
        EventStatusId = completed ? EventStatusCode.Completed : EventStatusCode.Cancelled,
        ImagePath = "uploads/events/11111111111111111111111111111111.jpg",
        CreatedBy = actorId,
        CreatedAt = now
    };

    private static Registration Registration(
        int eventId,
        int memberId,
        RegistrationStatusCode status,
        string courseSnapshot,
        int yearLevelSnapshot,
        int actorId,
        DateTime now) => new()
    {
        EventId = eventId,
        MemberId = memberId,
        RegistrationStatusId = status,
        CourseSnapshot = courseSnapshot,
        YearLevelSnapshot = yearLevelSnapshot,
        SectionSnapshot = "A",
        SubmittedAt = now,
        CreatedBy = actorId,
        CreatedAt = now
    };

    private static Attendance Attendance(int registrationId, int actorId, DateTime now, DateTime? invalidatedAt = null) => new()
    {
        RegistrationId = registrationId,
        CheckInMethodId = CheckInMethodCode.QrCode,
        RecordedBy = actorId,
        RecordedAt = now,
        InvalidatedBy = invalidatedAt is null ? null : actorId,
        InvalidatedAt = invalidatedAt,
        InvalidationReason = invalidatedAt is null ? null : "Test invalidation"
    };
}
