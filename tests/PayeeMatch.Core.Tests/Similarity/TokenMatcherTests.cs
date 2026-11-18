using PayeeMatch.Names;
using PayeeMatch.Similarity;

namespace PayeeMatch.Core.Tests.Similarity;

public class TokenMatcherTests
{
    private const int Precision = 3;

    [Fact]
    public void The_same_name_scores_one()
    {
        Assert.Equal(1, Match("Joao Manuel Silva", "Joao Manuel Silva").Score);
    }

    [Fact]
    public void Word_order_does_not_matter()
    {
        var match = Match("Silva Joao Manuel", "Joao Manuel Silva");

        Assert.Equal(1, match.Score);
        Assert.All(match.Pairs, p => Assert.Equal(TokenPairKind.Exact, p.Kind));
    }

    [Fact]
    public void Initials_pair_with_the_words_they_stand_for()
    {
        var match = Match("J. M. Silva", "Joao Manuel Silva");

        Assert.Equal(
            [("j", "joao", TokenPairKind.Initial), ("m", "manuel", TokenPairKind.Initial)],
            match.Pairs.Where(p => p.Kind == TokenPairKind.Initial).Select(p => (p.Left, p.Right, p.Kind)).OrderBy(p => p.Left));
        Assert.Equal((1 + 0.8 + 0.8) / 3, match.Score, Precision);
    }

    [Fact]
    public void A_missing_middle_name_lowers_the_score_but_does_not_fail_it()
    {
        var match = Match("Joao Silva", "Joao Manuel Silva");

        Assert.Equal(["manuel"], match.UnpairedRight);
        Assert.Equal(2 / 2.3, match.Score, Precision);
    }

    [Fact]
    public void A_different_first_name_counts_as_a_contradiction()
    {
        var match = Match("Maria Silva", "Joao Silva");

        Assert.Equal(["maria"], match.UnpairedLeft);
        Assert.Equal(["joao"], match.UnpairedRight);
        Assert.Equal(0.5, match.Score, Precision);
    }

    [Fact]
    public void A_typo_in_a_word_still_pairs_it()
    {
        var pair = Assert.Single(Match("Perreira", "Pereira").Pairs);

        Assert.Equal(TokenPairKind.Similar, pair.Kind);
        Assert.True(pair.Score is > 0.85 and < 1);
    }

    [Fact]
    public void Close_but_different_surnames_do_not_pair()
    {
        Assert.Empty(Match("Silva", "Sousa").Pairs);
    }

    [Fact]
    public void Initials_alone_score_zero()
    {
        var match = Match("J. M. S.", "Joao Manuel Silva");

        Assert.False(match.HasFullWordPair);
        Assert.Equal(0, match.Score);
    }

    [Fact]
    public void A_full_word_beats_an_initial_for_the_same_partner()
    {
        var match = Match("J. Joao Silva", "Joao Silva");

        Assert.Contains(match.Pairs, p => p is { Left: "joao", Right: "joao", Kind: TokenPairKind.Exact });
        Assert.Equal(["j"], match.UnpairedLeft);
    }

    [Fact]
    public void An_empty_name_scores_zero()
    {
        Assert.Equal(0, Match("", "Joao Silva").Score);
    }

    [Fact]
    public void A_question_mark_stands_for_one_letter_of_the_other_word()
    {
        var match = Match("Jo?o Silva", "Joao Silva");

        Assert.Contains(match.Pairs, p => p is { Left: "jo?o", Right: "joao", Kind: TokenPairKind.Placeholder });
        Assert.Equal((0.95 + 1) / 2, match.Score, Precision);
    }

    [Fact]
    public void Each_unknown_letter_costs_a_little()
    {
        var pair = Assert.Single(Match("Concei??o", "Conceicao").Pairs);

        Assert.Equal(TokenPairKind.Placeholder, pair.Kind);
        Assert.Equal(0.9, pair.Score, Precision);
    }

    [Fact]
    public void Known_letters_still_have_to_agree()
    {
        var match = Match("Jo?e", "Joao");

        Assert.DoesNotContain(match.Pairs, p => p.Kind == TokenPairKind.Placeholder);
    }

    [Fact]
    public void Words_of_different_lengths_fall_back_to_spelling_similarity()
    {
        var pair = Assert.Single(Match("Stra?e", "Strasse").Pairs);

        Assert.Equal(TokenPairKind.Similar, pair.Kind);
    }

    private static TokenMatch Match(string left, string right) =>
        TokenMatcher.Match(NameNormaliser.Normalise(left), NameNormaliser.Normalise(right));
}
