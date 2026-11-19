using System.Text.RegularExpressions;
using ASI.Basecode.Data.Models.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

/// <summary>
/// The dossier's fact sheet, roster caption and count are rendered contracts, not only styling.
/// </summary>
public sealed class EventDossierLayoutTests(SqlServerDatabaseFixture fixture)
    : IClassFixture<SqlServerDatabaseFixture>, IAsyncLifetime
{
    private const string Password = "DossierPassword#2026";

    private AttendTaWebApplicationFactory factory = null!;
    private HttpClient client = null!;
    private int eventId;
    private int memberId;
    private string memberEmail = null!;

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N");
        var admin = await MemberEventTestSupport.SeedUserAsync(
            fixture.ConnectionString, $"dossier-admin-{suffix}@attendta.test", Password, RoleCode.Admin, ct);
        var memberUser = await MemberEventTestSupport.SeedUserAsync(
            fixture.ConnectionString, $"dossier-member-{suffix}@attendta.test", Password, RoleCode.Member, ct);
        var member = await MemberEventTestSupport.SeedMemberAsync(
            fixture.ConnectionString, memberUser.Id, MembershipStatusCode.Active, ct);
        memberId = member.MemberId;
        memberEmail = memberUser.Email;
        var (typeId, venueId) = await MemberEventTestSupport.SeedReferencesAsync(fixture.ConnectionString, admin.Id, ct);
        var starts = new DateTime(2099, 11, 10, 1, 0, 0, DateTimeKind.Utc);
        var item = await MemberEventTestSupport.SeedEventAsync(
            fixture.ConnectionString, admin.Id, typeId, venueId, EventStatusCode.Published,
            starts, starts.AddDays(-1), 100, null, "Dossier layout assembly", ct);
        await MemberEventTestSupport.SeedRegistrationAsync(
            fixture.ConnectionString, item.EventId, member.MemberId, RegistrationStatusCode.Confirmed, admin.Id, ct);
        eventId = item.EventId;

        factory = new AttendTaWebApplicationFactory(fixture.ConnectionString);
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        await MemberEventTestSupport.SignInAsync(client, admin.Email, Password, ct);
    }

    public async ValueTask DisposeAsync()
    {
        client?.Dispose();
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }
    }

    [Fact]
    public async Task PopulatedRosterShowsCompactCountAndSecondaryCheckIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var html = await client.GetStringAsync($"/Event/Details?id={eventId}&status=confirmed", ct);

        Assert.DoesNotContain("Asia/Manila", html, StringComparison.Ordinal);
        Assert.Contains("Showing 1–1 of 1 matching confirmed registration", html, StringComparison.Ordinal);

        var checkInButton = Regex.Match(html, "<button[^>]*data-dialog-open=\"check-in-[0-9]+\"[^>]*>").Value;
        Assert.Contains("button-secondary", checkInButton, StringComparison.Ordinal);
        Assert.DoesNotContain("button-primary", checkInButton, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EmptyRosterShowsZeroCountWithoutPagination()
    {
        var ct = TestContext.Current.CancellationToken;
        var html = await client.GetStringAsync($"/Event/Details?id={eventId}&status=waitlisted", ct);

        Assert.DoesNotContain("<caption>", html, StringComparison.Ordinal);
        Assert.Contains("0 matching waitlisted registrations", html, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label=\"Pagination\"", html, StringComparison.Ordinal);
        Assert.Matches("Cancelled / declined\\s*<span class=\"filter-tab-count\">0</span>", html);
    }

    [Fact]
    public async Task CancelledCountAndDeclineReasonAppearInBothManagementViews()
    {
        var ct = TestContext.Current.CancellationToken;
        int registrationId;
        await using (var context = MemberEventTestSupport.Context(fixture.ConnectionString))
        {
            var registration = await context.Registrations.SingleAsync(r => r.EventId == eventId && r.MemberId == memberId, ct);
            registrationId = registration.RegistrationId;
            registration.RegistrationStatusId = RegistrationStatusCode.Pending;
            await context.SaveChangesAsync(ct);
        }

        var token = await MemberEventTestSupport.AntiforgeryTokenAsync(client, $"/RegistrationReview/Index?eventId={eventId}", ct);
        var response = await client.PostAsync("/RegistrationReview/Decline", MemberEventTestSupport.Form(new()
        {
            ["id"] = registrationId.ToString(), ["status"] = "pending", ["eventId"] = eventId.ToString(),
            ["reason"] = "Decline <reason> & details", ["__RequestVerificationToken"] = token
        }), ct);
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        var dossier = await client.GetStringAsync($"/Event/Details?id={eventId}&status=cancelled", ct);
        Assert.Matches("Cancelled / declined\\s*<span class=\"filter-tab-count\">1</span>", dossier);
        Assert.Contains("Decline &lt;reason&gt; &amp; details", dossier, StringComparison.Ordinal);
        var review = await client.GetStringAsync($"/RegistrationReview/Index?status=cancelled&eventId={eventId}", ct);
        Assert.Contains("Decline &lt;reason&gt; &amp; details", review, StringComparison.Ordinal);
        var filtered = await client.GetStringAsync($"/Event/Details?id={eventId}&status=cancelled&search=no-such-person", ct);
        Assert.Matches("Cancelled / declined\\s*<span class=\"filter-tab-count\">0</span>", filtered);
        Assert.DoesNotContain("Decline &lt;reason&gt;", filtered, StringComparison.Ordinal);
        using var memberClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await MemberEventTestSupport.SignInAsync(memberClient, memberEmail, Password, ct);
        var memberPage = await memberClient.GetStringAsync("/MyEvents/Registrations", ct);
        Assert.Contains("Decline &lt;reason&gt; &amp; details", memberPage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CancelledBadgeCountsAllMatchingPagesAndDoesNotInventDeclineReasons()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var i = 0; i < 6; i++)
        {
            var email = $"roster-cancelled-{Guid.NewGuid():N}@attendta.test";
            var user = await MemberEventTestSupport.SeedUserAsync(fixture.ConnectionString, email, Password, RoleCode.Member, ct);
            var member = await MemberEventTestSupport.SeedMemberAsync(fixture.ConnectionString, user.Id, MembershipStatusCode.Active, ct);
            await MemberEventTestSupport.SeedRegistrationAsync(fixture.ConnectionString, eventId, member.MemberId, RegistrationStatusCode.Cancelled, user.Id, ct);
        }

        var html = await client.GetStringAsync($"/Event/Details?id={eventId}&status=cancelled&pageSize=5&search=Dossier", ct);
        Assert.Matches("Cancelled / declined\\s*<span class=\"filter-tab-count\">6</span>", html);
        Assert.Contains("Showing 1–5 of 6 matching cancelled registrations", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Decline reason", html, StringComparison.Ordinal);
    }
}
