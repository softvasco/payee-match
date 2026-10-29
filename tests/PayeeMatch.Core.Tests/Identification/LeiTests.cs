using PayeeMatch.Identification;

namespace PayeeMatch.Core.Tests.Identification;

public class LeiTests
{
    // both pass the MOD 97-10 check, worked out with a separate script before this code existed
    [Theory]
    [InlineData("5493001KJTIIGC8Y1R12")]
    [InlineData("HWUPKR0MPOU8FGXBT394")]
    public void A_lei_with_valid_check_digits_parses(string value) =>
        Assert.Equal(value, Lei.Parse(value).Value);

    [Fact]
    public void Lower_case_and_surrounding_spaces_are_accepted_and_stored_upper_case() =>
        Assert.Equal("5493001KJTIIGC8Y1R12", Lei.Parse("  5493001kjtiigc8y1r12 ").Value);

    [Theory]
    [InlineData("5493001KJTIIGC8Y1R13", "last check digit off by one")]
    [InlineData("5493001KJTIIGC8Y1R21", "check digits swapped")]
    [InlineData("4593001KJTIIGC8Y1R12", "two characters swapped")]
    [InlineData("5493001KJTIIGC8Y1R1", "19 characters")]
    [InlineData("5493001KJTIIGC8Y1R123", "21 characters")]
    [InlineData("5493001KJTIIGC8Y1RA2", "a letter in the check digits")]
    [InlineData("5493001KJTIIGC8Y-R12", "punctuation")]
    [InlineData("5493001KJTIIGÇ8Y1R12", "a letter outside ASCII")]
    [InlineData("", "empty")]
    public void Anything_else_is_refused(string value, string why)
    {
        Assert.False(Lei.TryParse(value, out _), why);
        Assert.Throws<FormatException>(() => Lei.Parse(value));
    }

    [Fact]
    public void Null_is_not_a_lei() => Assert.False(Lei.TryParse(null, out _));

    [Fact]
    public void Two_leis_with_the_same_value_are_equal() =>
        Assert.Equal(Lei.Parse("5493001KJTIIGC8Y1R12"), Lei.Parse("5493001kjtiigc8y1r12"));
}
