using PayeeMatch.Decisions;

namespace PayeeMatch.Identification;

/// <summary>Checks a code sent instead of a name against the one on record for the IBAN.</summary>
public static class IdentificationCheck
{
    // a code is either the holder's or someone else's, so unlike names there is no close match
    public static PayeeDecision Decide(Lei provided, Lei? onRecord)
    {
        ArgumentNullException.ThrowIfNull(provided);

        if (onRecord is null)
        {
            return new PayeeDecision(MatchOutcome.NotApplicable, 0, [ReasonCode.IdentifierNotOnRecord]);
        }

        return provided == onRecord
            ? new PayeeDecision(MatchOutcome.Match, 1, [ReasonCode.IdentifierMatches])
            : new PayeeDecision(MatchOutcome.NoMatch, 0, [ReasonCode.IdentifierDiffers]);
    }
}
