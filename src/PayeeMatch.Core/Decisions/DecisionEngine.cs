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

        var leftParty = NoiseWords.Strip(NameNormaliser.Normalise(provided));
        var rightParty = NoiseWords.Strip(NameNormaliser.Normalise(onRecord));
        var (left, right) = (leftParty.Core, rightParty.Core);

        if (left.IsEmpty || right.IsEmpty)
        {
            return new PayeeDecision(MatchOutcome.NoMatch, 0, [ReasonCode.NameEmpty]);
        }

        var (score, reasons) = left.Value == right.Value
            ? (1, [ReasonCode.SameNameAfterNormalisation])
            : Score(left, right);

        // a missing legal form is common, but two different ones are usually two different companies
        if (leftParty.HasLegalForm && rightParty.HasLegalForm && leftParty.LegalForm != rightParty.LegalForm)
        {
            score = Math.Min(score, Thresholds.CloseMatch);
            reasons.Add(ReasonCode.LegalFormDiffers);
        }

        // one letter apart is often a different brand, so the payer has to look at a company name
        var isCompany = leftParty.HasLegalForm || rightParty.HasLegalForm;
        if (isCompany && reasons.Contains(ReasonCode.SpellingDiffers))
        {
            score = Math.Min(score, Thresholds.CloseMatch);
        }

        return new PayeeDecision(Thresholds.Classify(score), score, reasons);
    }

    private static (double Score, List<ReasonCode> Reasons) Score(NormalisedName left, NormalisedName right)
    {
        var match = TokenMatcher.Match(left, right);
        return (match.Score, Reasons(match));
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
