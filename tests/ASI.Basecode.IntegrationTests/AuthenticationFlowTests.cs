using System.Net;
using System.Text.RegularExpressions;
using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Services.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

/// <summary>
/// End-to-end cookie authentication and policy checks through the real MVC pipeline.
/// </summary>
public class AuthenticationFlowTests : IClassFixture<SqlServerDatabaseFixture>, IAsyncLifetime
{
    private const string AdminEmail = "flow-admin@attendta.test";
    private const string OfficerEmail = "flow-officer@attendta.test";
    private const string MemberEmail = "flow-member@attendta.test";
    private const string InactiveEmail = "flow-inactive@attendta.test";
    private const string Password = "FlowPassword#2026";
    private const string CookieName = "AttendTa.Auth";

    private readonly SqlServerDatabaseFixture fixture;
    private readonly Argon2PasswordHasher hasher = new();
    private AttendTaWebApplicationFactory factory;
    private HttpClient client;

    public AuthenticationFlowTests(SqlServerDatabaseFixture fixture)
    {
        this.fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        await SeedUsersAsync();

        factory = new AttendTaWebApplicationFactory(fixture.ConnectionString);
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
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
    public async Task AnonymousRequestToMemberRedirectsToLoginWithReturnUrl()
    {
        var response = await client.GetAsync("/Member", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = GetLocationPath(response);
        Assert.NotNull(location);
        Assert.StartsWith("/Account/Login", location, StringComparison.Ordinal);
        Assert.Contains("ReturnUrl=%2FMember", location, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AdministratorSignsInAndOpensMemberManagement()
    {
        var login = await SignInAsync(AdminEmail, Password);

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/Home/Index", GetLocationPath(login));
        Assert.Contains($"{CookieName}=", GetSetCookieHeader(login), StringComparison.Ordinal);

        var member = await client.GetAsync("/Member", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, member.StatusCode);
    }

    [Fact]
    public async Task OfficerOpensMemberManagement()
    {
        var login = await SignInAsync(OfficerEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var member = await client.GetAsync("/Member", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, member.StatusCode);
    }

    [Fact]
    public async Task MemberIsRedirectedToAccessDenied()
    {
        var login = await SignInAsync(MemberEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var member = await client.GetAsync("/Member", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, member.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", GetLocationPath(member), StringComparison.Ordinal);

        var accessDenied = await client.GetAsync("/Account/AccessDenied", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, accessDenied.StatusCode);
        Assert.Contains("Access denied", await accessDenied.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SignInIssuesANonPersistentEightHourCookie()
    {
        var login = await SignInAsync(AdminEmail, Password);

        var setCookie = GetSetCookieHeader(login);
        Assert.Contains($"{CookieName}=", setCookie, StringComparison.Ordinal);
        Assert.DoesNotContain("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("max-age=", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WrongPasswordShowsTheGenericMessageAndIssuesNoCookie()
    {
        var response = await SignInAsync(AdminEmail, "wrong-password");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain($"{CookieName}=", GetSetCookieHeader(response), StringComparison.Ordinal);
        Assert.Contains("Incorrect email or password.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnknownEmailShowsTheSameGenericMessageAsAWrongPassword()
    {
        var wrongPassword = await SignInAsync(AdminEmail, "wrong-password");
        var wrongPasswordHtml = await wrongPassword.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var unknownEmail = await SignInAsync("nobody@attendta.test", Password);
        var unknownEmailHtml = await unknownEmail.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, unknownEmail.StatusCode);
        Assert.DoesNotContain($"{CookieName}=", GetSetCookieHeader(unknownEmail), StringComparison.Ordinal);
        Assert.Contains("Incorrect email or password.", unknownEmailHtml, StringComparison.Ordinal);
        Assert.Equal(ExtractGenericFailure(wrongPasswordHtml), ExtractGenericFailure(unknownEmailHtml));
    }

    [Fact]
    public async Task InactiveAccountCannotSignIn()
    {
        var response = await SignInAsync(InactiveEmail, Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain($"{CookieName}=", GetSetCookieHeader(response), StringComparison.Ordinal);
        Assert.Contains("Incorrect email or password.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeactivatedSessionIsRejectedOnTheNextRequest()
    {
        var login = await SignInAsync(AdminEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Member", TestContext.Current.CancellationToken)).StatusCode);

        await SetActiveAsync(AdminEmail, false);
        try
        {
            var afterDeactivation = await client.GetAsync("/Member", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Redirect, afterDeactivation.StatusCode);
            Assert.StartsWith("/Account/Login", GetLocationPath(afterDeactivation), StringComparison.Ordinal);
        }
        finally
        {
            await SetActiveAsync(AdminEmail, true);
        }
    }

    [Fact]
    public async Task DemotedSessionIsRejectedOnTheNextRequest()
    {
        var login = await SignInAsync(AdminEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        await SetRoleAsync(AdminEmail, RoleCode.Member);
        try
        {
            var afterDemotion = await client.GetAsync("/Member", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Redirect, afterDemotion.StatusCode);
            Assert.StartsWith("/Account/Login", GetLocationPath(afterDemotion), StringComparison.Ordinal);
        }
        finally
        {
            await SetRoleAsync(AdminEmail, RoleCode.Admin);
        }
    }

    [Fact]
    public async Task GetLogoutIsNotAllowed()
    {
        var login = await SignInAsync(AdminEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var response = await client.GetAsync("/Account/Logout", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task PostLogoutWithAntiforgeryTokenSignsTheUserOut()
    {
        var login = await SignInAsync(AdminEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var token = await GetAntiforgeryTokenAsync("/Member");
        var logout = await client.PostAsync(
            "/Account/Logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        // Account/Login are the route defaults, so the generated login URL is "/".
        Assert.Equal("/", GetLocationPath(logout));

        var loginPage = await client.GetAsync("/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        Assert.Contains("Sign in", await loginPage.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);

        var afterLogout = await client.GetAsync("/Member", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.StartsWith("/Account/Login", GetLocationPath(afterLogout), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostLogoutWithoutAntiforgeryTokenIsRejected()
    {
        var login = await SignInAsync(AdminEmail, Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var logout = await client.PostAsync(
            "/Account/Logout",
            new FormUrlEncodedContent(new Dictionary<string, string>()),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, logout.StatusCode);
    }

    private async Task<HttpResponseMessage> SignInAsync(string email, string password)
    {
        var token = await GetAntiforgeryTokenAsync("/Account/Login");
        return await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["__RequestVerificationToken"] = token
            }),
            TestContext.Current.CancellationToken);
    }

    private async Task<string> GetAntiforgeryTokenAsync(string path)
    {
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var match = Regex.Match(html, "__RequestVerificationToken[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        Assert.True(match.Success, $"No antiforgery token was rendered on {path}.");
        return match.Groups[1].Value;
    }

    private static string? GetLocationPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        if (location is null)
        {
            return null;
        }

        return location.IsAbsoluteUri ? location.PathAndQuery : location.ToString();
    }

    private static string GetSetCookieHeader(HttpResponseMessage response)
    {
        return response.Headers.TryGetValues("Set-Cookie", out var values)
            ? string.Join("; ", values)
            : string.Empty;
    }

    private static string ExtractGenericFailure(string html)
    {
        var match = Regex.Match(html, "Incorrect email or password\\.");
        return match.Success ? match.Value : string.Empty;
    }

    private async Task SeedUsersAsync()
    {
        var options = new DbContextOptionsBuilder<AsiBasecodeDBContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
        await using var context = new AsiBasecodeDBContext(options);

        var existingEmails = await context.Users
            .Select(user => user.UserId)
            .ToListAsync(TestContext.Current.CancellationToken);

        var users = new[]
        {
            CreateUser(AdminEmail, "Flow Admin", RoleCode.Admin, isActive: true),
            CreateUser(OfficerEmail, "Flow Officer", RoleCode.Officer, isActive: true),
            CreateUser(MemberEmail, "Flow Member", RoleCode.Member, isActive: true),
            CreateUser(InactiveEmail, "Flow Inactive", RoleCode.Admin, isActive: false)
        };

        context.Users.AddRange(users.Where(user => !existingEmails.Contains(user.UserId)));

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private User CreateUser(string email, string name, RoleCode role, bool isActive)
    {
        return new User
        {
            UserId = email,
            Name = name,
            Password = hasher.Hash(Password),
            RoleId = role,
            IsActive = isActive
        };
    }

    private async Task SetActiveAsync(string email, bool isActive)
    {
        var options = new DbContextOptionsBuilder<AsiBasecodeDBContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
        await using var context = new AsiBasecodeDBContext(options);
        var user = await context.Users.SingleAsync(item => item.UserId == email, TestContext.Current.CancellationToken);
        user.IsActive = isActive;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task SetRoleAsync(string email, RoleCode role)
    {
        var options = new DbContextOptionsBuilder<AsiBasecodeDBContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
        await using var context = new AsiBasecodeDBContext(options);
        var user = await context.Users.SingleAsync(item => item.UserId == email, TestContext.Current.CancellationToken);
        user.RoleId = role;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
