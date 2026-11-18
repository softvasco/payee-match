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

    [Property(MaxTest = 1000)]
    public Property An_unknown_letter_scores_at_least_what_splitting_on_it_did() =>
        Prop.ForAll(Name, Gen.Choose(0, 1000).ToArbitrary(), (name, seed) =>
        {
            var (unknown, split) = ReplaceOneLetter(name, seed);
            return Score(unknown, name) >= Score(split, name) - Tolerance;
        });

    [Property(MaxTest = 1000)]
    public Property An_unknown_letter_never_scores_above_the_exact_word() =>
        Prop.ForAll(Name, Gen.Choose(0, 1000).ToArbitrary(), (name, seed) =>
        {
            var (unknown, _) = ReplaceOneLetter(name, seed);
            return Score(unknown, name) <= Score(name, name) + Tolerance;
        });

    // the same letter of the same word replaced by ? and, the old way, by a space
    private static (string[] Unknown, string[] Split) ReplaceOneLetter(string[] name, int seed)
    {
        var word = seed % name.Length;
        var letter = seed % name[word].Length;

        string[] With(char c) =>
            [.. name.Select((w, i) => i == word ? string.Concat(w.AsSpan(0, letter), c.ToString(), w.AsSpan(letter + 1)) : w)];

        return (With('?'), With(' '));
    }

    private static double Score(string[] left, string[] right) =>
        TokenMatcher.Match(NameNormaliser.Normalise(string.Join(' ', left)), NameNormaliser.Normalise(string.Join(' ', right))).Score;
}
