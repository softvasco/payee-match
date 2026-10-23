namespace PayeeMatch.Decisions;

/// <summary>The outcome, the score it came from and every reason that applied.</summary>
public sealed record PayeeDecision(MatchOutcome Outcome, double Score, IReadOnlyList<ReasonCode> Reasons);
