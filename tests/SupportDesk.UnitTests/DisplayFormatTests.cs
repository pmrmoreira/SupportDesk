using SupportDesk.Web.Infrastructure;

namespace SupportDesk.UnitTests;

public class DisplayFormatTests
{
    [Fact]
    public void Summer_time_in_Lisbon_is_one_hour_ahead_of_UTC()
    {
        var utc = new DateTimeOffset(2026, 9, 28, 9, 40, 0, TimeSpan.Zero);

        Assert.Equal("28/09/2026 10:40", DisplayFormat.DateTime(utc));
    }

    [Fact]
    public void Winter_time_in_Lisbon_is_UTC()
    {
        var utc = new DateTimeOffset(2026, 12, 15, 9, 40, 0, TimeSpan.Zero);

        Assert.Equal("15/12/2026 09:40", DisplayFormat.DateTime(utc));
    }
}
