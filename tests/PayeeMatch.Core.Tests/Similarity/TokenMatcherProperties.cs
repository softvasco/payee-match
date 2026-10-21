using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using PayeeMatch.Names;
using PayeeMatch.Similarity;

namespace PayeeMatch.Core.Tests.Similarity;

public class TokenMatcherProperties
{
    private const double Tolerance = 1e-9;

    private static readonly string[] Words =
        ["Joao", "J", "Manuel", "M", "Silva", "Silvia", "Sousa", "Pereira", "Perreira", "Ana", "A", "Costa", "Conceição"];

    private static readonly Arbitrary<string[]> Name = Gen.Elements(Words).NonEmptyListOf().Select(w => w.ToArray()).ToArbitrary();

    [Property(MaxTest = 1000)]
    public Property The_score_is_the_same_both_ways() =>
        Prop.ForAll(Name, Name, (left, right) =>
            Math.Abs(Score(left, right) - Score(right, left)) < Tolerance);

    [Property(MaxTest = 1000)]
    public Property Reordering_the_words_does_not_change_the_score() =>
        Prop.ForAll(Name, Name, (left, right) =>
            Math.Abs(Score(left, right) - Score([.. left.Reverse()], right)) < Tolerance);

    [Property(MaxTest = 1000)]
    public Property The_score_stays_between_zero_and_one() =>
        Prop.ForAll(Name, Name, (left, right) => Score(left, right) is >= 0 and <= 1);

    [Property(MaxTest = 500)]
    public Property A_name_with_a_full_word_scores_one_against_itself() =>
        Prop.ForAll(Name, name =>
            !name.Any(w => w.Length > 1) || Math.Abs(Score(name, name) - 1) < Tolerance);

    private static double Score(string[] left, string[] right) =>
        TokenMatcher.Match(NameNormaliser.Normalise(string.Join(' ', left)), NameNormaliser.Normalise(string.Join(' ', right))).Score;
}
