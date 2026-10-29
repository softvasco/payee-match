using PayeeMatch.Decisions;
using PayeeMatch.Identification;

namespace PayeeMatch.Core.Tests.Identification;

public class IdentificationCheckTests
{
    private static readonly Lei OnRecord = Lei.Parse("5493001KJTIIGC8Y1R12");
    private static readonly Lei Other = Lei.Parse("HWUPKR0MPOU8FGXBT394");

    [Fact]
    public void The_code_on_record_is_MTCH()
    {
        var decision = IdentificationCheck.Decide(Lei.Parse("5493001kjtiigc8y1r12"), OnRecord);

        Assert.Equal(MatchOutcome.Match, decision.Outcome);
        Assert.Equal("MTCH", decision.Outcome.ToEpcCode());
        Assert.Equal([ReasonCode.IdentifierMatches], decision.Reasons);
    }

    [Fact]
    public void Another_valid_code_is_NMTC_and_never_a_close_match()
    {
        var decision = IdentificationCheck.Decide(Other, OnRecord);

        Assert.Equal("NMTC", decision.Outcome.ToEpcCode());
        Assert.Equal([ReasonCode.IdentifierDiffers], decision.Reasons);
    }

    [Fact]
    public void An_account_with_no_code_on_record_is_NOAP()
    {
        var decision = IdentificationCheck.Decide(OnRecord, onRecord: null);

        Assert.Equal(MatchOutcome.NotApplicable, decision.Outcome);
        Assert.Equal("NOAP", decision.Outcome.ToEpcCode());
        Assert.Equal([ReasonCode.IdentifierNotOnRecord], decision.Reasons);
    }

    [Fact]
    public void Every_outcome_has_its_EPC_code() =>
        Assert.Equal(
            ["MTCH", "CMTC", "NMTC", "NOAP"],
            Enum.GetValues<MatchOutcome>().Select(o => o.ToEpcCode()));
}
