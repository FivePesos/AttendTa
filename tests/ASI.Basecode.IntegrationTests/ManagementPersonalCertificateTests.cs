using System.Net;
using System.Text;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public sealed class ManagementPersonalCertificateTests(SqlServerDatabaseFixture fixture) : IClassFixture<SqlServerDatabaseFixture>
{
    [Theory]
    [InlineData(RoleCode.Officer)]
    [InlineData(RoleCode.Admin)]
    public async Task ManagementUserCanOpenAndDownloadTheirOwnIssuedCertificate(RoleCode role)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new AttendTaWebApplicationFactory(fixture.ConnectionString);
        var (email, password, registrationId, _, pdfBytes) = await SeedParticipationAsync(factory, role, MembershipStatusCode.Active, ct);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await MemberEventTestSupport.SignInAsync(client, email, password, ct);

        using var inline = await client.GetAsync($"/MyEvents/Certificates/{registrationId}/Pdf", ct);
        Assert.Equal(HttpStatusCode.OK, inline.StatusCode);
        Assert.Equal("application/pdf", inline.Content.Headers.ContentType?.MediaType);
        Assert.Equal(pdfBytes, await inline.Content.ReadAsByteArrayAsync(ct));

        using var download = await client.GetAsync($"/MyEvents/Certificates/{registrationId}/Pdf?download=true", ct);
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal("attachment", download.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(pdfBytes, await download.Content.ReadAsByteArrayAsync(ct));
    }

    [Theory]
    [InlineData(RoleCode.Officer)]
    [InlineData(RoleCode.Admin)]
    public async Task InactiveManagementMemberCanSeeRetainedHistoryAndCertificate(RoleCode role)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new AttendTaWebApplicationFactory(fixture.ConnectionString);
        var (email, password, registrationId, title, pdfBytes) = await SeedParticipationAsync(factory, role, MembershipStatusCode.Inactive, ct);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await MemberEventTestSupport.SignInAsync(client, email, password, ct);

        foreach (var route in new[] { "/MyEvents/Registrations", "/MyEvents/Attendance", "/MyEvents/Certificates" })
        {
            using var response = await client.GetAsync(route, ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync(ct);
            Assert.Contains(title, html, StringComparison.Ordinal);
            Assert.DoesNotContain("A linked member profile is needed", html, StringComparison.Ordinal);
        }

        using var pdf = await client.GetAsync($"/MyEvents/Certificates/{registrationId}/Pdf", ct);
        Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        Assert.Equal(pdfBytes, await pdf.Content.ReadAsByteArrayAsync(ct));
    }

    private async Task<(string Email, string Password, int RegistrationId, string Title, byte[] PdfBytes)> SeedParticipationAsync(
        AttendTaWebApplicationFactory factory, RoleCode role, MembershipStatusCode membershipStatus, CancellationToken ct)
    {
        var email = $"personal-certificate-{Guid.NewGuid():N}@attendta.test";
        var password = MemberEventTestSupport.NewPassword();
        var user = await MemberEventTestSupport.SeedUserAsync(fixture.ConnectionString, email, password, role, ct);
        var member = await MemberEventTestSupport.SeedMemberAsync(fixture.ConnectionString, user.Id, membershipStatus, ct);
        var (typeId, venueId) = await MemberEventTestSupport.SeedReferencesAsync(fixture.ConnectionString, user.Id, ct);
        var title = $"Retained participation {Guid.NewGuid():N}";
        var startsAt = DateTime.UtcNow.AddDays(-10);
        var eventItem = await MemberEventTestSupport.SeedEventAsync(fixture.ConnectionString, user.Id, typeId, venueId,
            EventStatusCode.Published, startsAt, startsAt.AddDays(-1), 20, null, title, ct);
        var registration = await MemberEventTestSupport.SeedRegistrationAsync(fixture.ConnectionString, eventItem.EventId,
            member.MemberId, RegistrationStatusCode.Confirmed, user.Id, ct);
        var fileName = $"{Guid.NewGuid():N}.pdf";
        var bytes = Encoding.ASCII.GetBytes("%PDF-1.7\n% AttendTa certificate\n");
        var directory = Path.Combine(factory.SetupRoot, "certificates");
        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(Path.Combine(directory, fileName), bytes, ct);

        await using var context = MemberEventTestSupport.Context(fixture.ConnectionString);
        context.AttendanceRecords.Add(new Attendance
        {
            RegistrationId = registration.RegistrationId,
            CheckInMethodId = CheckInMethodCode.QrCode,
            RecordedBy = user.Id,
            RecordedAt = startsAt.AddMinutes(5)
        });
        context.Certificates.Add(new Certificate
        {
            RegistrationId = registration.RegistrationId,
            CertificateStatusId = CertificateStatusCode.Issued,
            CertificateNumber = $"AT-{startsAt:yyyy}-{registration.RegistrationId:D6}",
            RecipientNameSnapshot = user.Name,
            EventTitleSnapshot = title,
            EventStartsAtSnapshot = startsAt,
            FilePath = $"certificates/{fileName}",
            IssuedBy = user.Id,
            IssuedAt = startsAt.AddHours(2),
            CreatedBy = user.Id,
            CreatedAt = startsAt.AddHours(1)
        });
        await context.SaveChangesAsync(ct);
        return (email, password, registration.RegistrationId, title, bytes);
    }
}
