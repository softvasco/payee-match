using PayeeMatch.Names;
using PayeeMatch.Similarity;

namespace PayeeMatch.Decisions;

/// <summary>Compares the name a payer typed with the name on the account and decides MTCH, CMTC or NMTC.</summary>
public sealed class DecisionEngine(MatchThresholds thresholds)
{
    public DecisionEngine()
        : this(new MatchThresholds())
    {
    }

    public MatchThresholds Thresholds { get; } = thresholds ?? throw new ArgumentNullException(nameof(thresholds));

    public PayeeDecision Decide(string provided, string onRecord)
    {
        ArgumentNullException.ThrowIfNull(provided);
        ArgumentNullException.ThrowIfNull(onRecord);

        var left = NoiseWords.Strip(NameNormaliser.Normalise(provided)).Core;
        var right = NoiseWords.Strip(NameNormaliser.Normalise(onRecord)).Core;

        if (left.IsEmpty || right.IsEmpty)
        {
            return new PayeeDecision(MatchOutcome.NoMatch, 0, [ReasonCode.NameEmpty]);
        }

        if (left.Value == right.Value)
        {
            return new PayeeDecision(MatchOutcome.Match, 1, [ReasonCode.SameNameAfterNormalisation]);
        }

        var match = TokenMatcher.Match(left, right);
        return new PayeeDecision(Thresholds.Classify(match.Score), match.Score, Reasons(match));
    }

    private static List<ReasonCode> Reasons(TokenMatch match)
    {
        if (!match.HasFullWordPair)
        {
            return [ReasonCode.NoWordInCommon];
        }

        var reasons = new List<ReasonCode>();
        if (match.Pairs.Any(p => p.Kind == TokenPairKind.Initial))
        {
            reasons.Add(ReasonCode.InitialUsed);
        }

        if (match.Pairs.Any(p => p.Kind == TokenPairKind.Similar))
        {
            reasons.Add(ReasonCode.SpellingDiffers);
        }

        var (left, right) = (match.UnpairedLeft.Count, match.UnpairedRight.Count);
        if (left > 0 && right > 0)
        {
            reasons.Add(ReasonCode.WordsDiffer);
        }
        else if (left + right > 0)
        {
            reasons.Add(ReasonCode.WordsMissing);
        }

        // every word paired exactly, yet the names differ: only the order is left
        return reasons.Count > 0 ? reasons : [ReasonCode.WordOrderDiffers];
    }
}
