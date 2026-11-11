using System.Diagnostics.CodeAnalysis;

namespace PayeeMatch.Identification;

/// <summary>A Legal Entity Identifier (ISO 17442): 18 characters and two check digits.</summary>
public sealed record Lei
{
    private const int Length = 20;

    private Lei(string value) => Value = value;

    public string Value { get; }

    public static Lei Parse(string value) =>
        TryParse(value, out var lei) ? lei : throw new FormatException($"'{value}' is not a valid LEI.");

    /// <summary>Accepts lower case and outer spaces; people paste these from documents.</summary>
    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Lei? lei)
    {
        lei = null;
        var candidate = value?.Trim().ToUpperInvariant();
        if (candidate is not { Length: Length }
            || !candidate.All(char.IsAsciiLetterOrDigit)
            || !char.IsAsciiDigit(candidate[^1])
            || !char.IsAsciiDigit(candidate[^2])
            || !Mod97.IsValid(candidate))
        {
            return false;
        }

        lei = new Lei(candidate);
        return true;
    }

    public override string ToString() => Value;
}
