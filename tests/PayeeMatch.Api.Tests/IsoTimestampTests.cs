using PayeeMatch.Api.Verification;

namespace PayeeMatch.Api.Tests;

public class IsoTimestampTests
{
    // the valid column of the table in the EPC spec, section 4.2.9.2
    [Theory]
    [InlineData(465, "2025-07-10T14:36:25.465Z")]
    [InlineData(460, "2025-07-10T14:36:25.46Z")]
    [InlineData(400, "2025-07-10T14:36:25.4Z")]
    [InlineData(0, "2025-07-10T14:36:25Z")]
    public void Milliseconds_are_written_without_trailing_zeros(int milliseconds, string expected)
    {
        var value = new DateTimeOffset(2025, 7, 10, 14, 36, 25, milliseconds, TimeSpan.Zero);

        Assert.Equal(expected, IsoTimestamp.Format(value));
    }

    [Fact]
    public void A_local_time_is_written_in_utc()
    {
        var value = new DateTimeOffset(2025, 7, 10, 16, 36, 25, 123, TimeSpan.FromHours(2));

        Assert.Equal("2025-07-10T14:36:25.123Z", IsoTimestamp.Format(value));
    }
}
