using System.Collections.Frozen;

namespace PayeeMatch.Names;

/// <summary>Removes words that say nothing about who the payee is: leading titles and a trailing legal form.</summary>
public static class NoiseWords
{
    // "m" and "don" are left out on purpose: they are just as often an initial or a first name
    private static readonly FrozenSet<string> Titles = new[]
    {
        "dr", "dra", "eng", "enga", "engo", "prof", "sr", "sra", "srta", "dona",
        "mr", "mrs", "ms", "miss", "mx", "sir",
        "herr", "frau", "mme", "mlle", "dhr", "mevr", "sig", "sigra",
    }.ToFrozenSet(StringComparer.Ordinal);

    // keyed by tokens as the normaliser leaves them, so "S.A." arrives here as "s a"
    private static readonly FrozenDictionary<string, string> LegalForms = new Dictionary<string, string>
    {
        // Portugal
        ["lda"] = "lda",
        ["limitada"] = "lda",
        ["unipessoal lda"] = "unipessoal lda",
        ["sociedade unipessoal lda"] = "unipessoal lda",
        ["sa"] = "sa",
        ["s a"] = "sa",
        ["sociedade anonima"] = "sa",
        ["crl"] = "crl",

        // Spain
        ["sl"] = "sl",
        ["s l"] = "sl",
        ["slu"] = "slu",
        ["s l u"] = "slu",
        ["sociedad limitada"] = "sl",
        ["sociedad anonima"] = "sa",

        // Germany and Austria
        ["gmbh"] = "gmbh",
        ["ag"] = "ag",
        ["kg"] = "kg",
        ["gmbh co kg"] = "gmbh co kg",
        ["ug haftungsbeschrankt"] = "ug",
        ["ev"] = "ev",
        ["e v"] = "ev",

        // Netherlands and Belgium
        ["bv"] = "bv",
        ["b v"] = "bv",
        ["nv"] = "nv",
        ["n v"] = "nv",
        ["vof"] = "vof",
        ["bvba"] = "bvba",
        ["srl"] = "srl",
        ["s r l"] = "srl",

        // France
        ["sarl"] = "sarl",
        ["s a r l"] = "sarl",
        ["sas"] = "sas",
        ["s a s"] = "sas",
        ["sasu"] = "sasu",
        ["eurl"] = "eurl",

        // Italy
        ["spa"] = "spa",
        ["s p a"] = "spa",

        // UK and Ireland
        ["ltd"] = "ltd",
        ["limited"] = "ltd",
        ["plc"] = "plc",
        ["llp"] = "llp",
        ["dac"] = "dac",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly int LongestLegalForm = LegalForms.Keys.Max(k => k.Count(c => c == ' ') + 1);

    public static PartyName Strip(NormalisedName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var tokens = name.Tokens;
        var start = 0;
        while (start < tokens.Count && Titles.Contains(tokens[start]))
        {
            start++;
        }

        var (end, legalForm) = FindLegalForm(tokens, start);

        // "Dr." or "SA" on its own is the whole name, not noise around one
        if (end <= start)
        {
            return new PartyName(name, null);
        }

        var core = tokens.Skip(start).Take(end - start).ToArray();
        return new PartyName(new NormalisedName(name.Original, core), legalForm);
    }

    private static (int End, string? LegalForm) FindLegalForm(IReadOnlyList<string> tokens, int start)
    {
        for (var length = Math.Min(LongestLegalForm, tokens.Count - start); length > 0; length--)
        {
            var suffix = string.Join(' ', tokens.Skip(tokens.Count - length));
            if (LegalForms.TryGetValue(suffix, out var canonical))
            {
                return (tokens.Count - length, canonical);
            }
        }

        return (tokens.Count, null);
    }
}
