using PayeeMatch.Decisions;

namespace PayeeMatch.Core.Tests.Decisions;

public class DecisionEngineTests
{
    private readonly DecisionEngine _engine = new();

    [Fact]
    public void Case_accents_and_titles_alone_still_make_a_match()
    {
        var decision = _engine.Decide("DR. JOÃO SILVA", "Joao Silva");

        Assert.Equal(MatchOutcome.Match, decision.Outcome);
        Assert.Equal([ReasonCode.SameNameAfterNormalisation], decision.Reasons);
    }

    [Fact]
    public void The_same_words_in_another_order_are_a_match()
    {
        var decision = _engine.Decide("Silva Joao", "Joao Silva");

        Assert.Equal(MatchOutcome.Match, decision.Outcome);
        Assert.Equal([ReasonCode.WordOrderDiffers], decision.Reasons);
    }

    [Fact]
    public void Initials_for_the_given_names_are_a_close_match()
    {
        var decision = _engine.Decide("J. M. Silva", "Joao Manuel Silva");

        Assert.Equal(MatchOutcome.CloseMatch, decision.Outcome);
        Assert.Equal([ReasonCode.InitialUsed], decision.Reasons);
    }

    [Fact]
    public void A_missing_middle_name_is_a_close_match()
    {
        var decision = _engine.Decide("Joao Silva", "Joao Manuel Silva");

        Assert.Equal(MatchOutcome.CloseMatch, decision.Outcome);
        Assert.Equal([ReasonCode.WordsMissing], decision.Reasons);
    }

    [Fact]
    public void A_different_surname_is_no_match()
    {
        var decision = _engine.Decide("Joao Santos", "Joao Silva");

        Assert.Equal(MatchOutcome.NoMatch, decision.Outcome);
        Assert.Equal([ReasonCode.WordsDiffer], decision.Reasons);
    }

    [Fact]
    public void Initials_with_no_full_word_in_common_are_no_match()
    {
        var decision = _engine.Decide("J. M. S.", "Joao Manuel Silva");

        Assert.Equal(MatchOutcome.NoMatch, decision.Outcome);
        Assert.Equal(0, decision.Score);
        Assert.Equal([ReasonCode.NoWordInCommon], decision.Reasons);
    }

    [Fact]
    public void A_different_legal_form_is_at_best_a_close_match()
    {
        var decision = _engine.Decide("Padaria Central Lda", "Padaria Central, S.A.");

        Assert.Equal(MatchOutcome.CloseMatch, decision.Outcome);
        Assert.Equal([ReasonCode.SameNameAfterNormalisation, ReasonCode.LegalFormDiffers], decision.Reasons);
    }

    [Fact]
    public void A_legal_form_left_out_still_makes_a_match()
    {
        var decision = _engine.Decide("Padaria Central", "Padaria Central Lda");

        Assert.Equal(MatchOutcome.Match, decision.Outcome);
        Assert.Equal([ReasonCode.SameNameAfterNormalisation], decision.Reasons);
    }

    [Fact]
    public void Two_spellings_of_the_same_legal_form_are_a_match()
    {
        Assert.Equal(MatchOutcome.Match, _engine.Decide("Padaria Central Limitada", "PADARIA CENTRAL, LDA.").Outcome);
    }

    [Fact]
    public void A_different_legal_form_never_lifts_a_weak_name_to_a_close_match()
    {
        var decision = _engine.Decide("Padaria Norte Lda", "Padaria Central SA");

        Assert.Equal(MatchOutcome.NoMatch, decision.Outcome);
        Assert.Contains(ReasonCode.LegalFormDiffers, decision.Reasons);
    }

    [Theory]
    [InlineData("", "Joao Silva")]
    [InlineData("Joao Silva", "  ")]
    [InlineData("...", "Joao Silva")]
    public void An_empty_name_is_no_match(string provided, string onRecord)
    {
        var decision = _engine.Decide(provided, onRecord);

        Assert.Equal(MatchOutcome.NoMatch, decision.Outcome);
        Assert.Equal([ReasonCode.NameEmpty], decision.Reasons);
    }

    [Fact]
    public void A_typo_is_reported_even_when_the_score_still_makes_a_match()
    {
        var decision = _engine.Decide("Joao Silvva", "Joao Silva");

        Assert.Contains(ReasonCode.SpellingDiffers, decision.Reasons);
        Assert.Equal(MatchOutcome.Match, decision.Outcome);
    }

    [Fact]
    public void A_bank_that_wants_typos_confirmed_raises_the_match_threshold()
    {
        var strict = new DecisionEngine(new MatchThresholds(match: 1, closeMatch: 0.8));

        Assert.Equal(MatchOutcome.CloseMatch, strict.Decide("Joao Silvva", "Joao Silva").Outcome);
        Assert.Equal(MatchOutcome.Match, strict.Decide("Silva Joao", "Joao Silva").Outcome);
    }

    [Theory]
    [InlineData(0.9, 0.9)]
    [InlineData(0.8, 0.9)]
    [InlineData(1.1, 0.8)]
    [InlineData(0.9, 0)]
    public void Thresholds_must_be_ordered_and_between_zero_and_one(double match, double closeMatch) =>
        Assert.ThrowsAny<ArgumentException>(() => new MatchThresholds(match, closeMatch));

    [Theory]
    [InlineData(MatchOutcome.Match, "MTCH")]
    [InlineData(MatchOutcome.CloseMatch, "CMTC")]
    [InlineData(MatchOutcome.NoMatch, "NMTC")]
    public void Outcomes_map_to_the_epc_codes(MatchOutcome outcome, string code) =>
        Assert.Equal(code, outcome.ToEpcCode());
}
