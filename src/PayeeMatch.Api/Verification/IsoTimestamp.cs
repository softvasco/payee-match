using System.Globalization;

namespace PayeeMatch.Api.Verification;

/// <summary>The EPC ISODateTime form: UTC, milliseconds without trailing zeros.</summary>
public static class IsoTimestamp
{
    // FFF drops trailing zeros, and the dot with them when there are none (spec 4.2.9.2)
    public static string Format(DateTimeOffset value) =>
        value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFF'Z'", CultureInfo.InvariantCulture);
}
