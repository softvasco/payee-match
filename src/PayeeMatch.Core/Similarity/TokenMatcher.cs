using PayeeMatch.Names;

namespace PayeeMatch.Similarity;

/// <summary>How a token of one name was paired with a token of the other.</summary>
public enum TokenPairKind
{
    Exact,
    Similar,
    Initial,
}

public sealed record TokenPair(string Left, string Right, TokenPairKind Kind, double Score);

/// <summary>Two names compared word by word, with the pairs behind the score.</summary>
public sealed record TokenMatch(
    double Score,
    IReadOnlyList<TokenPair> Pairs,
    IReadOnlyList<string> UnpairedLeft,
    IReadOnlyList<string> UnpairedRight)
{
    public bool HasFullWordPair => Pairs.Any(p => p.Kind != TokenPairKind.Initial);
}

/// <summary>Compares names token by token, in any word order and with initials.</summary>
public static class TokenMatcher
{
    // below this two words are different names, not a typo
    private const double SimilarTokenThreshold = 0.85;

    private const double InitialScore = 0.8;

    // a middle name only one side has costs less than a word that contradicts the other side
    private const double MissingTokenWeight = 0.3;

    public static TokenMatch Match(NormalisedName left, NormalisedName right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var pairs = new List<TokenPair>();
        var usedLeft = new bool[left.Tokens.Count];
        var usedRight = new bool[right.Tokens.Count];

        // greedy is enough for a handful of tokens, and it lets a full word win over an initial
        var candidates =
            from i in Enumerable.Range(0, left.Tokens.Count)
            from j in Enumerable.Range(0, right.Tokens.Count)
            let pair = Pair(left.Tokens[i], right.Tokens[j])
            where pair is not null
            orderby pair.Score descending, pair.Kind
            select (i, j, pair);

        foreach (var (i, j, pair) in candidates)
        {
            if (usedLeft[i] || usedRight[j])
            {
                continue;
            }

            usedLeft[i] = usedRight[j] = true;
            pairs.Add(pair);
        }

        var unpairedLeft = left.Tokens.Where((_, i) => !usedLeft[i]).ToList();
        var unpairedRight = right.Tokens.Where((_, j) => !usedRight[j]).ToList();

        return new TokenMatch(Score(pairs, unpairedLeft.Count, unpairedRight.Count), pairs, unpairedLeft, unpairedRight);
    }

    private static TokenPair? Pair(string left, string right)
    {
        if (left == right)
        {
            return new TokenPair(left, right, TokenPairKind.Exact, 1);
        }

        if (IsInitialOf(left, right) || IsInitialOf(right, left))
        {
            return new TokenPair(left, right, TokenPairKind.Initial, InitialScore);
        }

        var similarity = JaroWinkler.Similarity(left, right);
        return similarity >= SimilarTokenThreshold && left.Length > 1 && right.Length > 1
            ? new TokenPair(left, right, TokenPairKind.Similar, similarity)
            : null;
    }

    private static bool IsInitialOf(string initial, string word) =>
        initial.Length == 1 && word.Length > 1 && word[0] == initial[0];

    private static double Score(List<TokenPair> pairs, int unpairedLeft, int unpairedRight)
    {
        // initials alone ("j m s" against "joao manuel silva") say too little about who it is
        if (!pairs.Any(p => p.Kind != TokenPairKind.Initial))
        {
            return 0;
        }

        var contradicting = Math.Min(unpairedLeft, unpairedRight);
        var missing = Math.Abs(unpairedLeft - unpairedRight);
        var weight = pairs.Count + contradicting + (MissingTokenWeight * missing);
        return pairs.Sum(p => p.Score) / weight;
    }
}
