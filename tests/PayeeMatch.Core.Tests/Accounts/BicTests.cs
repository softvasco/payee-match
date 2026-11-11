using PayeeMatch.Accounts;

namespace PayeeMatch.Core.Tests.Accounts;

public class BicTests
{
    [Theory]
    [InlineData("DEUTDEFF")]
    [InlineData("DEUTDEFF500")]
    [InlineData("BNPAFRPPXXX")]
    [InlineData("1A2BPTPL")]
    public void Eight_or_eleven_characters_with_a_country_in_the_middle_parse(string value) =>
        Assert.Equal(value, Bic.Parse(value).Value);

    [Fact]
    public void Lower_case_and_outer_spaces_are_accepted() =>
        Assert.Equal("DEUTDEFF500", Bic.Parse(" deutdeff500 ").Value);

    [Fact]
    public void The_branch_is_dropped_for_the_institution() =>
        Assert.Equal(Bic.Parse("BNPAFRPP").Institution, Bic.Parse("BNPAFRPPXXX").Institution);

    [Fact]
    public void The_country_is_the_fifth_and_sixth_characters() =>
        Assert.Equal("FR", Bic.Parse("BNPAFRPP").CountryCode);

    [Theory]
    [InlineData("DEUTDEF", "7 characters")]
    [InlineData("DEUTDEFF50", "10 characters")]
    [InlineData("DEUT1EFF", "a digit in the country")]
    [InlineData("DEUT DEFF", "a space inside")]
    [InlineData("", "empty")]
    public void Anything_else_is_refused(string value, string why)
    {
        Assert.False(Bic.TryParse(value, out _), why);
        Assert.Throws<FormatException>(() => Bic.Parse(value));
    }
}
