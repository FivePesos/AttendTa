using ASI.Basecode.Data;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.DemoData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public sealed class DemoCheckInWindowFlagTests(SqlServerDatabaseFixture fixture) : IClassFixture<SqlServerDatabaseFixture>
{
    [Fact]
    public async Task ScannerAndManualLookupReadTheRealCheckInFlag()
    {
        var ct = TestContext.Current.CancellationToken;
        var officerId = await EventDraftTestSupport.SeedActorAsync(fixture.ConnectionString, ct);
        var (typeId, venueId) = await MemberEventTestSupport.SeedReferencesAsync(fixture.ConnectionString, officerId, ct);
        var published = await MemberEventTestSupport.SeedEventAsync(
            fixture.ConnectionString, officerId, typeId, venueId, EventStatusCode.Published,
            DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(6), 20, null, "Check-in flag event", ct);

        await using var context = EventDraftTestSupport.Context(fixture.ConnectionString);
        var unitOfWork = new UnitOfWork(context);
        var events = new EventRepository(unitOfWork);
        var registrations = new RegistrationRepository(unitOfWork);
        var store = new DemoStore(Options.Create(new DemoDataOptions { Enabled = true, TokenKey = "test-token-key" }));
        var scanner = new DemoCheckInService(events, registrations, store, TimeProvider.System);
        var lookup = new DemoCheckInLookupService(events, registrations, store, TimeProvider.System);

        Assert.False((await scanner.GetScreenAsync(published.EventId, ct)).IsCheckInOpen);
        Assert.Equal(CheckInOutcomeViewModel.Closed, (await scanner.ScanAsync(published.EventId, "unknown", ct)).Outcome);
        Assert.Equal(CheckInOutcomeViewModel.Closed, (await lookup.CheckInAsync(published.EventId, 999, "unknown", ct)).Outcome);

        await using (var writer = EventDraftTestSupport.Context(fixture.ConnectionString))
        {
            var row = await writer.Events.SingleAsync(item => item.EventId == published.EventId, ct);
            row.IsCheckInOpen = true;
            await writer.SaveChangesAsync(ct);
        }

        Assert.True((await scanner.GetScreenAsync(published.EventId, ct)).IsCheckInOpen);
        Assert.Equal(CheckInOutcomeViewModel.Invalid, (await scanner.ScanAsync(published.EventId, "unknown", ct)).Outcome);
        Assert.Equal(CheckInOutcomeViewModel.Invalid, (await lookup.CheckInAsync(published.EventId, 999, "unknown", ct)).Outcome);
    }
}
