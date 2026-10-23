namespace PayeeMatch.Decisions;

/// <summary>The answer a Verification of Payee check gives, with the EPC scheme's code for each.</summary>
public enum MatchOutcome
{
    /// <summary>MTCH: the name belongs to the account holder.</summary>
    Match,

    /// <summary>CMTC: close enough that the payer should see the holder's name and decide.</summary>
    CloseMatch,

    /// <summary>NMTC: the name does not belong to the account holder.</summary>
    NoMatch,
}

public static class MatchOutcomeCodes
{
    public static string ToEpcCode(this MatchOutcome outcome) => outcome switch
    {
        MatchOutcome.Match => "MTCH",
        MatchOutcome.CloseMatch => "CMTC",
        MatchOutcome.NoMatch => "NMTC",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null),
    };
}
