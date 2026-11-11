using System.Diagnostics.CodeAnalysis;

namespace PayeeMatch.Accounts;

/// <summary>A BIC (ISO 9362): 8 characters, or 11 with a branch code.</summary>
public sealed record Bic
{
    private Bic(string value) => Value = value;

    public string Value { get; }

    public string CountryCode => Value[4..6];

    /// <summary>The first 8 characters, the same for every branch of one institution.</summary>
    public string Institution => Value[..8];

    public static Bic Parse(string value) =>
        TryParse(value, out var bic) ? bic : throw new FormatException($"'{value}' is not a valid BIC.");

    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Bic? bic)
    {
        bic = null;
        var candidate = value?.Trim().ToUpperInvariant();

        // since the 2022 edition the first four characters may hold digits too
        if (candidate is not { Length: 8 or 11 }
            || !candidate.All(char.IsAsciiLetterOrDigit)
            || !char.IsAsciiLetterUpper(candidate[4])
            || !char.IsAsciiLetterUpper(candidate[5]))
        {
            return false;
        }

        bic = new Bic(candidate);
        return true;
    }

    public override string ToString() => Value;
}
