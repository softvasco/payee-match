using PayeeMatch.Names;

namespace PayeeMatch.Core.Tests.Names;

public class NoiseWordsTests
{
    [Theory]
    [InlineData("Dr. Ana Silva", "ana silva")]
    [InlineData("Engº João Costa", "joao costa")]
    [InlineData("Sra. D. Maria Santos", "d maria santos")]
    [InlineData("Mr Peter Smith", "peter smith")]
    [InlineData("Herr Dr. Jürgen Müller", "jurgen muller")]
    [InlineData("Mevr. de Vries", "de vries")]
    public void Drops_leading_titles(string input, string expected)
    {
        var stripped = Strip(input);

        Assert.Equal(expected, stripped.Core.Value);
        Assert.Null(stripped.LegalForm);
    }

    [Theory]
    [InlineData("Silva & Filhos, Lda.", "silva filhos", "lda")]
    [InlineData("Padaria Central Unipessoal Lda", "padaria central", "unipessoal lda")]
    [InlineData("Banco Exemplo, S.A.", "banco exemplo", "sa")]
    [InlineData("Talleres García S.L.", "talleres garcia", "sl")]
    [InlineData("Müller Bau GmbH & Co. KG", "muller bau", "gmbh co kg")]
    [InlineData("Bakkerij Jansen B.V.", "bakkerij jansen", "bv")]
    [InlineData("Boulangerie Martin SARL", "boulangerie martin", "sarl")]
    [InlineData("Acme Widgets Limited", "acme widgets", "ltd")]
    [InlineData("Ferrari S.p.A.", "ferrari", "spa")]
    public void Takes_the_legal_form_off_the_end(string input, string expectedCore, string expectedForm)
    {
        var stripped = Strip(input);

        Assert.Equal(expectedCore, stripped.Core.Value);
        Assert.Equal(expectedForm, stripped.LegalForm);
    }

    [Theory]
    [InlineData("Silva Lda", "Silva Limitada")]
    [InlineData("Acme Ltd", "Acme Limited")]
    [InlineData("Exemplo SA", "Exemplo, S.A.")]
    [InlineData("Exemplo SA", "Exemplo Sociedade Anónima")]
    public void Spellings_of_the_same_legal_form_agree(string left, string right)
    {
        Assert.Equal(Strip(left), Strip(right));
    }

    [Theory]
    [InlineData("SA Santos Pereira", "sa santos pereira")]
    [InlineData("Sal Ltd Unipessoal", "sal ltd unipessoal")]
    public void Leaves_legal_form_words_alone_anywhere_but_the_end(string input, string expectedCore)
    {
        Assert.Equal(expectedCore, Strip(input).Core.Value);
    }

    [Theory]
    [InlineData("Dr.")]
    [InlineData("S.A.")]
    [InlineData("Ltd")]
    public void Keeps_a_name_that_is_nothing_but_noise(string input)
    {
        var stripped = Strip(input);

        Assert.Equal(NameNormaliser.Normalise(input), stripped.Core);
        Assert.Null(stripped.LegalForm);
    }

    [Fact]
    public void Keeps_the_original_text_for_the_audit_trail()
    {
        Assert.Equal("Dr. Ana Silva", Strip("Dr. Ana Silva").Core.Original);
    }

    private static PartyName Strip(string name) => NoiseWords.Strip(NameNormaliser.Normalise(name));
}
