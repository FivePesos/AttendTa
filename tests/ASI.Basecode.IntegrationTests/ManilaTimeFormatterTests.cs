using ASI.Basecode.WebApp.Formatting;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public class ManilaTimeFormatterTests
{
    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void UtcDatabaseTimestampRollsOverToNextManilaDay(DateTimeKind kind)
    {
        var timestamp = new DateTime(2026, 1, 1, 18, 30, 0, kind);
        Assert.Equal("2026-01-02 02:30 Asia/Manila", ManilaTimeFormatter.Format(timestamp));
        Assert.Equal("2026-01-02 02:30 Asia/Manila", ManilaTimeFormatter.Format((DateTime?)timestamp));
    }

    [Fact]
    public void OffsetIsConvertedAsAnInstant()
    {
        var timestamp = new DateTimeOffset(2026, 1, 1, 13, 30, 0, TimeSpan.FromHours(-5));
        Assert.Equal("2026-01-02 02:30 Asia/Manila", ManilaTimeFormatter.Format(timestamp));
        Assert.Equal("2026-01-02 02:30 Asia/Manila", ManilaTimeFormatter.Format((DateTimeOffset?)timestamp));
    }

    [Fact]
    public void LocalDateTimeIsRejectedInsteadOfBeingMisinterpretedAsUtc()
    {
        var timestamp = new DateTime(2026, 1, 1, 18, 30, 0, DateTimeKind.Local);
        Assert.Throws<ArgumentException>(() => ManilaTimeFormatter.Format(timestamp));
        Assert.Throws<ArgumentException>(() => ManilaTimeFormatter.Format((DateTime?)timestamp));
    }

    [Fact]
    public void NullTimestampsAreEmpty()
    {
        Assert.Equal(string.Empty, ManilaTimeFormatter.Format((DateTime?)null));
        Assert.Equal(string.Empty, ManilaTimeFormatter.Format((DateTimeOffset?)null));
    }
}
