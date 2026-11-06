using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;
using Xunit;

namespace ASI.Basecode.Services.Tests;

public sealed class RegistrationReviewTests
{
    [Theory]
    [InlineData(null, RegistrationStatusCode.Pending)]
    [InlineData("pending", RegistrationStatusCode.Pending)]
    [InlineData(" CONFIRMED ", RegistrationStatusCode.Confirmed)]
    [InlineData("waitlisted", RegistrationStatusCode.Waitlisted)]
    [InlineData("cancelled", RegistrationStatusCode.Cancelled)]
    public void QueueStatusIsNormalized(string? status, RegistrationStatusCode expected) =>
        Assert.Equal(expected, RegistrationReviewService.ParseStatus(status));

    [Theory]
    [InlineData(false, "admin")]
    [InlineData(true, "member")]
    public async Task ReviewCommandsRejectNonManagementBeforeAccessingData(bool authenticated, string role)
    {
        var actor = new Actor(authenticated, role);
        var service = new RegistrationReviewService(null!, null!, null!, null!, null!, actor, null!, null!, TimeProvider.System);
        var error = await Assert.ThrowsAsync<DomainException>(() => service.ApproveAsync(1, TestContext.Current.CancellationToken));
        Assert.Equal(OutcomeCode.Forbidden, error.Outcome);
    }

    private sealed record Actor(bool IsAuthenticated, string Role) : ICurrentActor
    {
        public int UserId => 1;
    }
}
