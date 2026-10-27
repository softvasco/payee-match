namespace PayeeMatch.Decisions;

/// <summary>The decision for the holder whose name came closest, and that holder's name as it is on record.</summary>
public sealed record HolderDecision(string Holder, PayeeDecision Decision);
