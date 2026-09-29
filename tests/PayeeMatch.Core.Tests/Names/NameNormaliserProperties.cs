using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using PayeeMatch.Names;

namespace PayeeMatch.Core.Tests.Names;

public class NameNormaliserProperties
{
    // random unicode rarely hits the cases that matter, so also build names from the letters banks actually see
    private const string NameLike = "aeiouAEIOU ãáàâçéêíóõôúüñÃÁÇÉÓÜÑßøØłŁæÆœþ'’`.-,&  ";

    private static readonly Arbitrary<string> NameLikeStrings =
        Gen.Elements(NameLike.ToCharArray()).ArrayOf().Select(chars => new string(chars)).ToArbitrary();

    [Property(MaxTest = 1000)]
    public Property Normalising_a_name_like_string_twice_changes_nothing() =>
        Prop.ForAll(NameLikeStrings, name =>
        {
            var once = NameNormaliser.Normalise(name);
            return NameNormaliser.Normalise(once.Value) == once;
        });

    [Property(MaxTest = 1000)]
    public bool Normalising_twice_changes_nothing(NonNull<string> name)
    {
        var once = NameNormaliser.Normalise(name.Get);

        return NameNormaliser.Normalise(once.Value) == once;
    }

    [Property(MaxTest = 1000)]
    public bool Output_is_lower_case_single_spaced_and_trimmed(NonNull<string> name)
    {
        var value = NameNormaliser.Normalise(name.Get).Value;

        return !value.Any(char.IsUpper)
            && !value.Contains("  ", StringComparison.Ordinal)
            && value == value.Trim();
    }
}
