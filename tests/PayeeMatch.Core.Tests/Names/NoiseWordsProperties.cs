using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using PayeeMatch.Names;

namespace PayeeMatch.Core.Tests.Names;

public class NoiseWordsProperties
{
    private static readonly string[] Titles = ["Dr.", "Eng.", "Sra.", "Mr", "Herr", "Prof.", "Mevr."];
    private static readonly string[] Words = ["Ana", "Silva", "Costa", "Müller", "Jansen", "Pereira", "Conceição"];
    private static readonly string[] LegalForms = ["Lda", "S.A.", "GmbH", "B.V.", "Ltd", "SARL", ""];

    private static readonly Arbitrary<(string[] Titles, string[] Core, string LegalForm)> Names =
        (from titles in Gen.SubListOf(Titles)
         from core in Gen.Elements(Words).NonEmptyListOf()
         from legalForm in Gen.Elements(LegalForms)
         select (titles.ToArray(), core.ToArray(), legalForm)).ToArbitrary();

    [Property(MaxTest = 500)]
    public Property The_order_of_titles_does_not_change_the_result() =>
        Prop.ForAll(Names, name =>
        {
            var inOrder = Strip(name.Titles, name.Core, name.LegalForm);
            var reversed = Strip(name.Titles.Reverse(), name.Core, name.LegalForm);
            return inOrder == reversed;
        });

    [Property(MaxTest = 500)]
    public Property Titles_and_legal_forms_never_reach_the_core() =>
        Prop.ForAll(Names, name =>
        {
            var stripped = Strip(name.Titles, name.Core, name.LegalForm);
            return stripped.Core == NameNormaliser.Normalise(string.Join(' ', name.Core));
        });

    private static PartyName Strip(IEnumerable<string> titles, IEnumerable<string> core, string legalForm) =>
        NoiseWords.Strip(NameNormaliser.Normalise(string.Join(' ', titles.Concat(core).Append(legalForm))));
}
