using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Security;
using ASI.Basecode.WebApp.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public sealed class CertificateTracerFlowTests(SqlServerDatabaseFixture fixture) : IClassFixture<SqlServerDatabaseFixture>
{
    [Fact]
    public async Task TracerRequiresConfirmedRegistrationAndManagementInDevelopmentAndDeletesStreamedPdf()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var suffix = Guid.NewGuid().ToString("N");
        var officerEmail = $"certificate-officer-{suffix}@attendta.test";
        var memberEmail = $"certificate-member-{suffix}@attendta.test";
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)) + "aA1!";
        var hasher = new Argon2PasswordHasher();
        var options = new DbContextOptionsBuilder<AsiBasecodeDBContext>().UseSqlServer(fixture.ConnectionString).Options;
        int officerId;
        int registrationId;

        await using (var context = new AsiBasecodeDBContext(options))
        {
            var officer = CreateUser(officerEmail, RoleCode.Officer, hasher, password);
            var memberUser = CreateUser(memberEmail, RoleCode.Member, hasher, password);
            context.Users.AddRange(officer, memberUser);
            await context.SaveChangesAsync(cancellationToken);
            officerId = officer.Id;

            var member = new Member
            {
                UserId = memberUser.Id,
                StudentNumber = $"TRACER-{suffix[..12]}",
                Course = "Computer Science",
                YearLevel = 2,
                Section = "A",
                MembershipStatusId = MembershipStatusCode.Active,
                CreatedBy = officerId
            };
            var eventType = new EventType
            {
                TypeName = $"Tracer Type {suffix[..12]}",
                Description = "Integration test event type.",
                IsActive = true,
                CreatedBy = officerId
            };
            var venue = new Venue
            {
                RoomName = $"Tracer Venue {suffix[..12]}",
                Capacity = 40,
                IsActive = true,
                ImagePath = "uploads/venues/tracer-test.jpg",
                CreatedBy = officerId
            };
            context.Members.Add(member);
            context.EventTypes.Add(eventType);
            context.Venues.Add(venue);
            await context.SaveChangesAsync(cancellationToken);

            var startsAt = DateTime.UtcNow.AddDays(3);
            var eventItem = new Event
            {
                Title = $"Tracer Event {suffix[..12]}",
                Description = "Integration test event.",
                TypeId = eventType.TypeId,
                VenueId = venue.VenueId,
                StartsAt = startsAt,
                RegistrationDeadline = startsAt.AddDays(-1),
                ParticipantLimit = 40,
                EventStatusId = EventStatusCode.Published,
                PublishedBy = officerId,
                PublishedAt = DateTime.UtcNow,
                CreatedBy = officerId
            };
            context.Events.Add(eventItem);
            await context.SaveChangesAsync(cancellationToken);

            var registration = new Registration
            {
                MemberId = member.MemberId,
                EventId = eventItem.EventId,
                RegistrationStatusId = RegistrationStatusCode.Confirmed,
                CourseSnapshot = member.Course,
                YearLevelSnapshot = member.YearLevel,
                SectionSnapshot = member.Section,
                ConfirmedBy = officerId,
                ConfirmedAt = DateTime.UtcNow,
                CreatedBy = officerId
            };
            context.Registrations.Add(registration);
            await context.SaveChangesAsync(cancellationToken);
            registrationId = registration.RegistrationId;
            Assert.Empty(await context.Certificates.ToListAsync(cancellationToken));
        }

        var developmentProbe = new StorageProbe();
        using var developmentFactory = CreateFactory(fixture.ConnectionString, "Development", developmentProbe);
        using var officerClient = developmentFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        using var anonymousClient = developmentFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

        Assert.Equal(HttpStatusCode.Redirect, (await anonymousClient.GetAsync("/CertificateTracer", cancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await PostTraceAsync(anonymousClient, registrationId, "", cancellationToken)).StatusCode);
        var officerSignIn = await SignInAsync(officerClient, officerEmail, password, cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, officerSignIn.StatusCode);

        var tracerPage = await officerClient.GetAsync("/CertificateTracer", cancellationToken);
        Assert.True(
            tracerPage.StatusCode == HttpStatusCode.OK,
            $"Tracer page returned {(int)tracerPage.StatusCode}; environment={developmentFactory.Services.GetRequiredService<IWebHostEnvironment>().EnvironmentName}; sign-in location={officerSignIn.Headers.Location}; response={await tracerPage.Content.ReadAsStringAsync(cancellationToken)}");
        var tracerToken = GetAntiforgeryToken(await tracerPage.Content.ReadAsStringAsync(cancellationToken));

        using (var noTokenForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["registrationId"] = registrationId.ToString(CultureInfo.InvariantCulture)
        }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, (await officerClient.PostAsync("/CertificateTracer/Trace", noTokenForm, cancellationToken)).StatusCode);
        }

        var unknownRegistration = await PostTraceAsync(officerClient, registrationId + 999999, tracerToken, cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unknownRegistration.StatusCode);
        Assert.Null(developmentProbe.PromotedKey);

        var pdfResponse = await PostTraceAsync(officerClient, registrationId, tracerToken, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
        Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);
        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdfBytes, 0, 5));
        AssertLandscapeA4(pdfBytes);
        var promotedKey = Assert.IsType<string>(developmentProbe.PromotedKey);
        Assert.Matches(new Regex(@"^certificates/[a-f0-9]{32}\.pdf$", RegexOptions.CultureInvariant), promotedKey);
        Assert.Contains(promotedKey, developmentProbe.DeletedPaths);
        Assert.Empty(Directory.GetFiles(Path.Combine(developmentFactory.SetupRoot, "certificates")));

        await using (var context = new AsiBasecodeDBContext(options))
        {
            Assert.Empty(await context.Certificates.ToListAsync(cancellationToken));
        }

        using var memberClient = developmentFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        Assert.Equal(HttpStatusCode.Redirect, (await SignInAsync(memberClient, memberEmail, password, cancellationToken)).StatusCode);
        var deniedMember = await memberClient.GetAsync("/CertificateTracer", cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, deniedMember.StatusCode);
        Assert.Contains("/Account/AccessDenied", deniedMember.Headers.Location?.ToString(), StringComparison.Ordinal);
        var memberHome = await memberClient.GetAsync("/Home/Index", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, memberHome.StatusCode);
        var memberToken = GetAntiforgeryToken(await memberHome.Content.ReadAsStringAsync(cancellationToken));
        var deniedMemberPost = await PostTraceAsync(memberClient, registrationId, memberToken, cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, deniedMemberPost.StatusCode);
        Assert.Contains("/Account/AccessDenied", deniedMemberPost.Headers.Location?.ToString(), StringComparison.Ordinal);
        Assert.Single(developmentProbe.DeletedPaths);

        var productionProbe = new StorageProbe();
        using var productionFactory = CreateFactory(fixture.ConnectionString, "Production", productionProbe);
        using var productionOfficer = productionFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        var productionLoginPage = await productionOfficer.GetAsync("/Account/Login", cancellationToken);
        var productionLoginToken = GetAntiforgeryToken(await productionLoginPage.Content.ReadAsStringAsync(cancellationToken));
        var productionSignIn = await productionOfficer.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = officerEmail,
            ["Password"] = password,
            ["__RequestVerificationToken"] = productionLoginToken
        }), cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, productionSignIn.StatusCode);
        var productionHome = await productionOfficer.GetAsync("/Home/Index", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, productionHome.StatusCode);
        var authenticatedProductionToken = GetAntiforgeryToken(await productionHome.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, (await productionOfficer.GetAsync("/CertificateTracer", cancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await PostTraceAsync(productionOfficer, registrationId, authenticatedProductionToken, cancellationToken)).StatusCode);
        Assert.Null(productionProbe.PromotedKey);

        const string physicalPath = "C:\\private\\certificate-tracer-secret.pdf";
        var failureStorageProbe = new StorageProbe();
        var failureLogProbe = new TracerLogProbe();
        using var failureFactory = CreateFactory(
            fixture.ConnectionString,
            "Development",
            failureStorageProbe,
            new FailingPdfRenderer(physicalPath),
            failureLogProbe);
        using var failureClient = failureFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        Assert.Equal(HttpStatusCode.Redirect, (await SignInAsync(failureClient, officerEmail, password, cancellationToken)).StatusCode);
        var failurePage = await failureClient.GetAsync("/CertificateTracer", cancellationToken);
        var failureToken = GetAntiforgeryToken(await failurePage.Content.ReadAsStringAsync(cancellationToken));
        var safeFailure = await PostTraceAsync(failureClient, registrationId, failureToken, cancellationToken);
        Assert.Equal(HttpStatusCode.InternalServerError, safeFailure.StatusCode);
        var safeFailureBody = await safeFailure.Content.ReadAsStringAsync(cancellationToken);
        Assert.DoesNotContain(physicalPath, safeFailureBody, StringComparison.OrdinalIgnoreCase);
        var failureLog = Assert.Single(
            failureLogProbe.Entries,
            entry => entry.Category == "ASI.Basecode.WebApp.Controllers.CertificateTracerController" && entry.Level == LogLevel.Error);
        Assert.Equal("Development certificate PDF tracer failed.", failureLog.Message);
        Assert.Null(failureLog.Exception);
        Assert.DoesNotContain(physicalPath, failureLog.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(failureStorageProbe.PromotedKey);

        await using (var context = new AsiBasecodeDBContext(options))
        {
            var registration = await context.Registrations.SingleAsync(item => item.RegistrationId == registrationId, cancellationToken);
            registration.RegistrationStatusId = RegistrationStatusCode.Pending;
            await context.SaveChangesAsync(cancellationToken);
        }

        var unconfirmedRegistration = await PostTraceAsync(officerClient, registrationId, tracerToken, cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unconfirmedRegistration.StatusCode);
        Assert.Single(developmentProbe.DeletedPaths);

        await using (var context = new AsiBasecodeDBContext(options))
        {
            var officer = await context.Users.SingleAsync(user => user.Id == officerId, cancellationToken);
            officer.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
        }

        var inactiveResponse = await PostTraceAsync(officerClient, registrationId, tracerToken, cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, inactiveResponse.StatusCode);
        Assert.Contains("/Account/Login", inactiveResponse.Headers.Location?.ToString(), StringComparison.Ordinal);
        Assert.Single(developmentProbe.DeletedPaths);
    }

    private static AttendTaWebApplicationFactory CreateFactory(
        string connectionString,
        string environmentName,
        StorageProbe probe,
        ICertificatePdfRenderer? renderer = null,
        TracerLogProbe? logProbe = null)
    {
        var factory = new AttendTaWebApplicationFactory(connectionString, environmentName: environmentName)
        {
            ConfigureServicesForTesting = services =>
            {
                services.RemoveAll<IPrivateFileStore>();
                services.AddSingleton(probe);
                services.AddScoped<IPrivateFileStore>(provider => new TrackingPrivateFileStore(
                    provider.GetRequiredService<IConfiguration>(),
                    provider.GetRequiredService<IWebHostEnvironment>(),
                    provider.GetRequiredService<StorageProbe>()));
                if (renderer is not null)
                {
                    services.RemoveAll<ICertificatePdfRenderer>();
                    services.AddSingleton<ICertificatePdfRenderer>(renderer);
                }

                if (logProbe is not null)
                {
                    services.AddLogging(builder => builder.AddProvider(logProbe));
                }
            }
        };
        return factory;
    }

    private static User CreateUser(string email, RoleCode role, Argon2PasswordHasher hasher, string password) => new()
    {
        Name = "Certificate tracer test user",
        UserId = email,
        Password = hasher.Hash(password),
        RoleId = role,
        IsActive = true
    };

    private static async Task<HttpResponseMessage> SignInAsync(HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
        var loginPage = await client.GetAsync("/Account/Login", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        var token = GetAntiforgeryToken(await loginPage.Content.ReadAsStringAsync(cancellationToken));
        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        }), cancellationToken);
    }

    private static Task<HttpResponseMessage> PostTraceAsync(HttpClient client, int registrationId, string token, CancellationToken cancellationToken) =>
        client.PostAsync("/CertificateTracer/Trace", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["registrationId"] = registrationId.ToString(CultureInfo.InvariantCulture),
            ["__RequestVerificationToken"] = token
        }), cancellationToken);

    private static string GetAntiforgeryToken(string html)
    {
        var match = Regex.Match(html, "__RequestVerificationToken[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            match = Regex.Match(html, "<meta\\s+name=\"RequestVerificationToken\"\\s+content=\"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        Assert.True(match.Success, "No antiforgery token was rendered on the form.");
        return match.Groups[1].Value;
    }

    private static void AssertLandscapeA4(byte[] pdf)
    {
        var text = Encoding.ASCII.GetString(pdf);
        var match = Regex.Match(text, @"/MediaBox\s*\[\s*(?<x0>-?[\d.]+)\s+(?<y0>-?[\d.]+)\s+(?<x1>-?[\d.]+)\s+(?<y1>-?[\d.]+)\s*\]", RegexOptions.CultureInvariant);
        Assert.True(match.Success, "The rendered PDF did not contain a readable MediaBox.");
        var width = double.Parse(match.Groups["x1"].Value, CultureInfo.InvariantCulture) - double.Parse(match.Groups["x0"].Value, CultureInfo.InvariantCulture);
        var height = double.Parse(match.Groups["y1"].Value, CultureInfo.InvariantCulture) - double.Parse(match.Groups["y0"].Value, CultureInfo.InvariantCulture);
        Assert.True(width > height, $"Expected landscape PDF dimensions but got {width} × {height} points.");
        Assert.InRange(Math.Abs(width - 841.89), 0, 1);
        Assert.InRange(Math.Abs(height - 595.28), 0, 1);
    }

    private sealed class StorageProbe
    {
        public string? PromotedKey { get; set; }
        public List<string> DeletedPaths { get; } = new();
    }

    private sealed class FailingPdfRenderer(string physicalPath) : ICertificatePdfRenderer
    {
        public Task<byte[]> RenderSampleAsync(CancellationToken cancellationToken = default) =>
            Task.FromException<byte[]>(new IOException($"Renderer could not write {physicalPath}."));
    }

    private sealed record CapturedLog(string Category, LogLevel Level, string Message, Exception? Exception);

    private sealed class TracerLogProbe : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedLog> entries = new();

        public IReadOnlyList<CapturedLog> Entries => entries.ToArray();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, entries);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string categoryName, ConcurrentQueue<CapturedLog> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                {
                    entries.Enqueue(new CapturedLog(categoryName, logLevel, formatter(state, exception), exception));
                }
            }
        }
    }

    private sealed class TrackingPrivateFileStore : IPrivateFileStore
    {
        private readonly PrivateFileStore inner;
        private readonly StorageProbe probe;

        public TrackingPrivateFileStore(IConfiguration configuration, IWebHostEnvironment environment, StorageProbe probe)
        {
            inner = new PrivateFileStore(configuration, environment);
            this.probe = probe;
        }

        public Task<string> SaveAsync(PrivateFileArea area, Stream content, string extension, CancellationToken cancellationToken = default) =>
            inner.SaveAsync(area, content, extension, cancellationToken);

        public string CreateTemporaryPath(string extension) => inner.CreateTemporaryPath(extension);

        public async Task<string> PromoteAsync(string temporaryPath, PrivateFileArea area, CancellationToken cancellationToken = default)
        {
            var key = await inner.PromoteAsync(temporaryPath, area, cancellationToken);
            probe.PromotedKey = key;
            return key;
        }

        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default) => inner.OpenReadAsync(relativePath, cancellationToken);

        public void Delete(string relativePath)
        {
            probe.DeletedPaths.Add(relativePath);
            inner.Delete(relativePath);
        }
    }
}
