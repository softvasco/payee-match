namespace PayeeMatch.Similarity;

/// <summary>Jaro and Jaro-Winkler similarity, from 0 (nothing in common) to 1 (identical).</summary>
public static class JaroWinkler
{
    private const double PrefixScale = 0.1;
    private const int MaxPrefixLength = 4;

    // Winkler only boosted pairs that were already fairly close; below this the shared prefix is luck
    private const double BoostThreshold = 0.7;

    public static double Similarity(string first, string second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var jaro = Jaro(first, second);
        if (jaro <= BoostThreshold)
        {
            return jaro;
        }

        var prefix = CommonPrefixLength(first, second);
        return jaro + (prefix * PrefixScale * (1 - jaro));
    }

    public static double Jaro(string first, string second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (first.Length == 0 && second.Length == 0)
        {
            return 1;
        }

        if (first.Length == 0 || second.Length == 0)
        {
            return 0;
        }

        var window = Math.Max(0, (Math.Max(first.Length, second.Length) / 2) - 1);
        var firstMatched = new bool[first.Length];
        var secondMatched = new bool[second.Length];
        var matches = 0;

        for (var i = 0; i < first.Length; i++)
        {
            var from = Math.Max(0, i - window);
            var to = Math.Min(second.Length - 1, i + window);
            for (var j = from; j <= to; j++)
            {
                if (!secondMatched[j] && first[i] == second[j])
                {
                    firstMatched[i] = secondMatched[j] = true;
                    matches++;
                    break;
                }
            }
        }

        if (matches == 0)
        {
            return 0;
        }

        var halfTranspositions = 0;
        var k = 0;
        for (var i = 0; i < first.Length; i++)
        {
            if (!firstMatched[i])
            {
                continue;
            }

            while (!secondMatched[k])
            {
                k++;
            }

            if (first[i] != second[k])
            {
                halfTranspositions++;
            }

            k++;
        }

        double m = matches;
        return ((m / first.Length) + (m / second.Length) + ((m - (halfTranspositions / 2)) / m)) / 3;
    }

    private static int CommonPrefixLength(string first, string second)
    {
        var limit = Math.Min(MaxPrefixLength, Math.Min(first.Length, second.Length));
        var length = 0;
        while (length < limit && first[length] == second[length])
        {
            length++;
        }

        return length;
    }
}
