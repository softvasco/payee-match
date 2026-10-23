namespace PayeeMatch.Decisions;

/// <summary>The scores at which a name counts as a match or a close match. Each bank picks its own.</summary>
public sealed record MatchThresholds
{
    public MatchThresholds(double match = 0.95, double closeMatch = 0.8)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(closeMatch, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(match, 1);
        if (closeMatch >= match)
        {
            throw new ArgumentException($"The close match threshold ({closeMatch}) has to be below the match threshold ({match}).");
        }

        Match = match;
        CloseMatch = closeMatch;
    }

    public double Match { get; }

    public double CloseMatch { get; }

    public MatchOutcome Classify(double score) =>
        score >= Match ? MatchOutcome.Match
        : score >= CloseMatch ? MatchOutcome.CloseMatch
        : MatchOutcome.NoMatch;
}
