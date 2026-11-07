using PayeeMatch.Identification;

namespace PayeeMatch.Accounts;

/// <summary>What the bank holds for an IBAN: its holders in the order on record, and the LEI if it has one.</summary>
public sealed record AccountRecord
{
    public AccountRecord(string iban, IReadOnlyList<string> holders, Lei? lei = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iban);
        ArgumentNullException.ThrowIfNull(holders);
        if (holders.Count == 0 || holders.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("An account has at least one holder, and every holder has a name.", nameof(holders));
        }

        Iban = IbanFormat.Electronic(iban);
        Holders = [.. holders];
        Lei = lei;
    }

    public string Iban { get; }

    public IReadOnlyList<string> Holders { get; }

    public Lei? Lei { get; }
}
