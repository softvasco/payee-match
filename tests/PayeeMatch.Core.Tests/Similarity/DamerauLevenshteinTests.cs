using PayeeMatch.Similarity;

namespace PayeeMatch.Core.Tests.Similarity;

public class DamerauLevenshteinTests
{
    [Theory]
    [InlineData("kitten", "sitting", 3)]
    [InlineData("sunday", "saturday", 3)]
    [InlineData("costa", "cotsa", 1)]
    [InlineData("a cat", "an act", 2)]
    [InlineData("ca", "abc", 2)]
    [InlineData("silva", "silva", 0)]
    [InlineData("", "silva", 5)]
    [InlineData("silva", "", 5)]
    [InlineData("", "", 0)]
    public void Matches_the_reference_distances(string first, string second, int expected) =>
        Assert.Equal(expected, DamerauLevenshtein.Distance(first, second));

    [Fact]
    public void A_swap_costs_one_edit_where_plain_levenshtein_needs_two()
    {
        Assert.Equal(1, DamerauLevenshtein.Distance("pereira", "pereria"));
    }

    [Theory]
    [InlineData("kitten", "sitting")]
    [InlineData("ca", "abc")]
    [InlineData("joao", "joana")]
    public void Does_not_depend_on_argument_order(string first, string second) =>
        Assert.Equal(DamerauLevenshtein.Distance(first, second), DamerauLevenshtein.Distance(second, first));

    [Theory]
    [InlineData("silva", "silva", 1.0)]
    [InlineData("", "", 1.0)]
    [InlineData("abc", "xyz", 0.0)]
    [InlineData("costa", "cotsa", 0.8)]
    [InlineData("kitten", "sitting", 0.571)]
    public void Similarity_scales_by_the_longer_string(string first, string second, double expected) =>
        Assert.Equal(expected, DamerauLevenshtein.Similarity(first, second), 3);
}
