namespace PayeeMatch.Identification;

/// <summary>ISO 7064 MOD 97-10, the check behind IBANs and LEIs.</summary>
internal static class Mod97
{
    // letters count as 10 to 35, and a valid code leaves 1; works digit by digit so no big integers
    public static bool IsValid(IEnumerable<char> upperCaseAlphanumeric)
    {
        var remainder = 0;
        foreach (var c in upperCaseAlphanumeric)
        {
            remainder = char.IsAsciiDigit(c)
                ? ((remainder * 10) + (c - '0')) % 97
                : ((remainder * 100) + (c - 'A' + 10)) % 97;
        }

        return remainder == 1;
    }
}
