using System.Collections.Frozen;
using System.Globalization;
using System.Text;

namespace PayeeMatch.Names;

/// <summary>Strips everything that shouldn't decide a match: case, accents, punctuation and spacing.</summary>
public static class NameNormaliser
{
    // letters that NFKD leaves alone because they aren't an accent on a base letter
    private static readonly FrozenDictionary<char, string> Folds = new Dictionary<char, string>
    {
        ['ß'] = "ss", ['æ'] = "ae", ['œ'] = "oe", ['ø'] = "o", ['ł'] = "l",
        ['đ'] = "d", ['ð'] = "d", ['þ'] = "th", ['ı'] = "i", ['ŀ'] = "l",
    }.ToFrozenDictionary();

    public static NormalisedName Normalise(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var tokens = new List<string>();
        var current = new StringBuilder();

        foreach (var c in name.Normalize(NormalizationForm.FormKD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var lower = char.ToLowerInvariant(c);
            if (Folds.TryGetValue(lower, out var folded))
            {
                current.Append(folded);
            }
            else if (char.IsLetterOrDigit(lower))
            {
                current.Append(lower);
            }
            else if (IsJoiner(lower))
            {
                // O'Neill and D'Almeida are one word, not two
                continue;
            }
            else
            {
                Flush(current, tokens);
            }
        }

        Flush(current, tokens);
        return new NormalisedName(name, tokens);
    }

    private static bool IsJoiner(char c) => c is '\'' or '\u2019' or '\u02bc' or '`';

    private static void Flush(StringBuilder current, List<string> tokens)
    {
        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
            current.Clear();
        }
    }
}
