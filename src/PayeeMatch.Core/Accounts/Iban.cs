using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using PayeeMatch.Identification;

namespace PayeeMatch.Accounts;

/// <summary>An IBAN (ISO 13616) from a SEPA country, in its electronic form.</summary>
public sealed record Iban
{
    // from the SWIFT IBAN registry; Verification of Payee only runs inside SEPA
    private static readonly FrozenDictionary<string, int> SepaLengths = new Dictionary<string, int>
    {
        ["AD"] = 24, ["AT"] = 20, ["BE"] = 16, ["BG"] = 22, ["CH"] = 21, ["CY"] = 28, ["CZ"] = 24, ["DE"] = 22,
        ["DK"] = 18, ["EE"] = 20, ["ES"] = 24, ["FI"] = 18, ["FR"] = 27, ["GB"] = 22, ["GI"] = 23, ["GR"] = 27,
        ["HR"] = 21, ["HU"] = 28, ["IE"] = 22, ["IS"] = 26, ["IT"] = 27, ["LI"] = 21, ["LT"] = 20, ["LU"] = 20,
        ["LV"] = 21, ["MC"] = 27, ["MT"] = 31, ["NL"] = 18, ["NO"] = 15, ["PL"] = 28, ["PT"] = 25, ["RO"] = 24,
        ["SE"] = 24, ["SI"] = 19, ["SK"] = 24, ["SM"] = 27, ["VA"] = 22,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private Iban(string value) => Value = value;

    public string Value { get; }

    public string CountryCode => Value[..2];

    public static Iban Parse(string value) =>
        TryParse(value, out var iban) ? iban : throw new FormatException($"'{value}' is not a valid SEPA IBAN.");

    /// <summary>Accepts the printed form with spaces and lower case, as people type it.</summary>
    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Iban? iban)
    {
        iban = null;
        if (value is null)
        {
            return false;
        }

        var candidate = IbanFormat.Electronic(value);
        if (candidate.Length < 4
            || !char.IsAsciiLetterUpper(candidate[0])
            || !char.IsAsciiLetterUpper(candidate[1])
            || !SepaLengths.TryGetValue(candidate[..2], out var length)
            || candidate.Length != length
            || !char.IsAsciiDigit(candidate[2])
            || !char.IsAsciiDigit(candidate[3])
            || !candidate.All(char.IsAsciiLetterOrDigit)
            || !Mod97.IsValid(candidate[4..].Concat(candidate[..4])))
        {
            return false;
        }

        iban = new Iban(candidate);
        return true;
    }

    public override string ToString() => Value;
}
