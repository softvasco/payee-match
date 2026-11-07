namespace PayeeMatch.Accounts;

/// <summary>A fixed set of accounts, for tests, demos and the quickstart.</summary>
public sealed class InMemoryAccountDirectory : IAccountDirectory
{
    private readonly Dictionary<string, AccountRecord> _accounts;

    public InMemoryAccountDirectory(IEnumerable<AccountRecord> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);

        _accounts = new Dictionary<string, AccountRecord>(StringComparer.Ordinal);
        foreach (var account in accounts)
        {
            if (!_accounts.TryAdd(account.Iban, account))
            {
                throw new ArgumentException($"IBAN {account.Iban} is listed twice.", nameof(accounts));
            }
        }
    }

    public Task<AccountRecord?> FindAsync(string iban, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(iban);
        return Task.FromResult(_accounts.GetValueOrDefault(IbanFormat.Electronic(iban)));
    }
}
