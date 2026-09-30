using PayeeMatch.Similarity;

namespace PayeeMatch.Core.Tests.Similarity;

public class JaroWinklerTests
{
    private const int Precision = 3;

    // the worked examples from Winkler's papers, as most implementations quote them
    [Theory]
    [InlineData("martha", "marhta", 0.944, 0.961)]
    [InlineData("dwayne", "duane", 0.822, 0.840)]
    [InlineData("dixon", "dicksonx", 0.767, 0.813)]
    public void Matches_the_reference_values(string first, string second, double jaro, double jaroWinkler)
    {
        Assert.Equal(jaro, JaroWinkler.Jaro(first, second), Precision);
        Assert.Equal(jaroWinkler, JaroWinkler.Similarity(first, second), Precision);
    }

    [Theory]
    [InlineData("silva", "silva")]
    [InlineData("", "")]
    public void Identical_strings_score_one(string first, string second) =>
        Assert.Equal(1, JaroWinkler.Similarity(first, second));

    [Theory]
    [InlineData("abc", "xyz")]
    [InlineData("silva", "")]
    [InlineData("", "silva")]
    public void Nothing_in_common_scores_zero(string first, string second) =>
        Assert.Equal(0, JaroWinkler.Similarity(first, second));

    [Theory]
    [InlineData("martha", "marhta")]
    [InlineData("pereira", "perreira")]
    [InlineData("joao costa", "costa joao")]
    public void Does_not_depend_on_argument_order(string first, string second) =>
        Assert.Equal(JaroWinkler.Similarity(first, second), JaroWinkler.Similarity(second, first), 10);

    [Fact]
    public void A_shared_prefix_lifts_close_pairs_only()
    {
        Assert.True(JaroWinkler.Similarity("silva", "silvia") > JaroWinkler.Jaro("silva", "silvia"));

        // too far apart for the prefix to count
        Assert.Equal(JaroWinkler.Jaro("abcdxyz", "abcdmnopqrstuvw"), JaroWinkler.Similarity("abcdxyz", "abcdmnopqrstuvw"));
    }

    [Fact]
    public void Characters_outside_the_match_window_do_not_count()
    {
        // the 'a's are eight apart and the window for these lengths is three
        Assert.Equal(0, JaroWinkler.Jaro("abbbbbbb", "cccccccca"));
    }
}
