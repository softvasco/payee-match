namespace PayeeMatch.Similarity;

/// <summary>Edit distance where swapping two characters counts as one edit, like a typo usually is.</summary>
public static class DamerauLevenshtein
{
    // the full Lowrance-Wagner version, not optimal string alignment: "ca" to "abc" is 2 here, 3 with OSA
    public static int Distance(string first, string second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (first.Length == 0 || second.Length == 0)
        {
            return Math.Max(first.Length, second.Length);
        }

        var lastRowWith = new Dictionary<char, int>();
        var maxDistance = first.Length + second.Length;
        var d = new int[first.Length + 2, second.Length + 2];

        d[0, 0] = maxDistance;
        for (var i = 0; i <= first.Length; i++)
        {
            d[i + 1, 0] = maxDistance;
            d[i + 1, 1] = i;
        }

        for (var j = 0; j <= second.Length; j++)
        {
            d[0, j + 1] = maxDistance;
            d[1, j + 1] = j;
        }

        for (var i = 1; i <= first.Length; i++)
        {
            var lastMatchingColumn = 0;
            for (var j = 1; j <= second.Length; j++)
            {
                var k = lastRowWith.GetValueOrDefault(second[j - 1]);
                var l = lastMatchingColumn;
                var cost = 1;
                if (first[i - 1] == second[j - 1])
                {
                    cost = 0;
                    lastMatchingColumn = j;
                }

                d[i + 1, j + 1] = Min(
                    d[i, j] + cost,
                    d[i + 1, j] + 1,
                    d[i, j + 1] + 1,
                    d[k, l] + (i - k - 1) + 1 + (j - l - 1));
            }

            lastRowWith[first[i - 1]] = i;
        }

        return d[first.Length + 1, second.Length + 1];
    }

    /// <summary>The distance scaled to 0..1 by the longer string, so it can sit next to Jaro-Winkler.</summary>
    public static double Similarity(string first, string second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var longest = Math.Max(first.Length, second.Length);
        return longest == 0 ? 1 : 1 - ((double)Distance(first, second) / longest);
    }

    private static int Min(int a, int b, int c, int d) => Math.Min(Math.Min(a, b), Math.Min(c, d));
}
