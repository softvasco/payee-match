namespace PayeeMatch.Names;

/// <summary>A name reduced to lower-case tokens with no accents or punctuation, ready to compare.</summary>
public sealed record NormalisedName
{
    internal NormalisedName(string original, IReadOnlyList<string> tokens)
    {
        Original = original;
        Tokens = tokens;
        Value = string.Join(' ', tokens);
    }

    public string Original { get; }

    public string Value { get; }

    public IReadOnlyList<string> Tokens { get; }

    public bool IsEmpty => Tokens.Count == 0;

    public bool Equals(NormalisedName? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;
}
