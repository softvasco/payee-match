namespace PayeeMatch.Accounts;

/// <summary>Finds the account behind an IBAN. Each bank plugs in its own core system here.</summary>
public interface IAccountDirectory
{
    /// <summary>Null when the bank has no such account, which the API answers with NOAP.</summary>
    Task<AccountRecord?> FindAsync(string iban, CancellationToken cancellationToken);
}
