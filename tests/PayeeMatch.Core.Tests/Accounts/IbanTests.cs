using PayeeMatch.Accounts;

namespace PayeeMatch.Core.Tests.Accounts;

public class IbanTests
{
    // the usual published examples, each checked with a separate script before this code existed
    [Theory]
    [InlineData("PT50000201231234567890154")]
    [InlineData("ES9121000418450200051332")]
    [InlineData("DE89370400440532013000")]
    [InlineData("FR1420041010050500013M02606")]
    [InlineData("NL91ABNA0417164300")]
    [InlineData("BE68539007547034")]
    [InlineData("GB82WEST12345698765432")]
    public void A_sepa_iban_with_valid_check_digits_parses(string value) =>
        Assert.Equal(value, Iban.Parse(value).Value);

    [Fact]
    public void The_printed_form_with_spaces_and_lower_case_is_accepted() =>
        Assert.Equal("PT50000201231234567890154", Iban.Parse("pt50 0002 0123 1234 5678 9015 4").Value);

    [Fact]
    public void The_country_is_the_first_two_letters() =>
        Assert.Equal("FR", Iban.Parse("FR1420041010050500013M02606").CountryCode);

    [Theory]
    [InlineData("PT50000201231234567890155", "last digit off by one")]
    [InlineData("PT05000201231234567890154", "check digits swapped")]
    [InlineData("DE89370400440532031000", "two digits swapped")]
    [InlineData("PT5000020123123456789015", "24 characters for Portugal")]
    [InlineData("PT500002012312345678901540", "26 characters for Portugal")]
    [InlineData("US64SVBKUS6S3300958879", "outside SEPA")]
    [InlineData("PTA0000201231234567890154", "a letter in the check digits")]
    [InlineData("PT50-0002-0123-1234-5678-9015-4", "dashes")]
    [InlineData("PT", "country only")]
    [InlineData("", "empty")]
    public void Anything_else_is_refused(string value, string why)
    {
        Assert.False(Iban.TryParse(value, out _), why);
        Assert.Throws<FormatException>(() => Iban.Parse(value));
    }

    [Fact]
    public void Null_is_refused_without_throwing() =>
        Assert.False(Iban.TryParse(null, out _));
}
