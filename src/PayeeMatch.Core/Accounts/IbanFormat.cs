namespace PayeeMatch.Accounts;

public static class IbanFormat
{
    /// <summary>The IBAN without spaces and in upper case, the form it is stored and looked up in.</summary>
    public static string Electronic(string iban)
    {
        ArgumentNullException.ThrowIfNull(iban);
        return string.Concat(iban.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    }
}
