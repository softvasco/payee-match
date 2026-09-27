using PayeeMatch.Names;

namespace PayeeMatch.Core.Tests.Names;

public class NameNormaliserTests
{
    [Theory]
    [InlineData("João Conceição", "joao conceicao")]
    [InlineData("MÜLLER Jürgen", "muller jurgen")]
    [InlineData("Françoise Lefèvre", "francoise lefevre")]
    [InlineData("Ñúñez Peña", "nunez pena")]
    [InlineData("Dvořák Šťastný", "dvorak stastny")]
    public void Removes_accents_and_case(string input, string expected)
    {
        Assert.Equal(expected, NameNormaliser.Normalise(input).Value);
    }

    [Theory]
    [InlineData("Straße", "strasse")]
    [InlineData("Søren Kierkegaard", "soren kierkegaard")]
    [InlineData("Łukasz Wałęsa", "lukasz walesa")]
    [InlineData("Ægir Þórsson", "aegir thorsson")]
    public void Folds_letters_that_have_no_accent_to_strip(string input, string expected)
    {
        Assert.Equal(expected, NameNormaliser.Normalise(input).Value);
    }

    [Theory]
    [InlineData("  Ana   Maria\tSilva ", "ana maria silva")]
    [InlineData("Silva-Santos, Ana", "silva santos ana")]
    [InlineData("J.M. Silva", "j m silva")]
    [InlineData("Silva & Filhos, Lda.", "silva filhos lda")]
    public void Splits_on_punctuation_and_collapses_spaces(string input, string expected)
    {
        Assert.Equal(expected, NameNormaliser.Normalise(input).Value);
    }

    [Theory]
    [InlineData("O'Neill", "oneill")]
    [InlineData("D’Almeida", "dalmeida")]
    public void Keeps_apostrophe_names_as_one_word(string input, string expected)
    {
        Assert.Equal(expected, NameNormaliser.Normalise(input).Value);
    }

    [Fact]
    public void Full_width_and_ligature_forms_become_plain_letters()
    {
        Assert.Equal("file abc", NameNormaliser.Normalise("ﬁle ＡＢＣ").Value);
    }

    [Fact]
    public void Exposes_tokens_and_keeps_the_original()
    {
        var name = NameNormaliser.Normalise("Ana-Maria Sousa");

        Assert.Equal(["ana", "maria", "sousa"], name.Tokens);
        Assert.Equal("Ana-Maria Sousa", name.Original);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" - . , ")]
    public void Nothing_left_means_empty(string input)
    {
        Assert.True(NameNormaliser.Normalise(input).IsEmpty);
    }

    [Fact]
    public void Names_that_normalise_the_same_are_equal()
    {
        Assert.Equal(NameNormaliser.Normalise("JOÃO SILVA"), NameNormaliser.Normalise("joao  silva"));
    }
}
