using Npgsql;
using PayeeMatch.Accounts;
using PayeeMatch.Identification;

namespace PayeeMatch.PostgreSql;

/// <summary>Reads accounts from the tables in <see cref="Schema"/>.</summary>
public sealed class PostgreSqlAccountDirectory(NpgsqlDataSource dataSource) : IAccountDirectory
{
    // position keeps the holder order on record, which decides a tie between joint holders
    public const string Schema = """
        CREATE TABLE IF NOT EXISTS accounts (
            iban varchar(34) PRIMARY KEY,
            lei char(20));
        CREATE TABLE IF NOT EXISTS account_holders (
            iban varchar(34) NOT NULL REFERENCES accounts (iban),
            position smallint NOT NULL,
            name varchar(140) NOT NULL,
            PRIMARY KEY (iban, position));
        """;

    private const string FindSql = """
        SELECT a.lei, h.name
        FROM accounts a JOIN account_holders h ON h.iban = a.iban
        WHERE a.iban = @iban
        ORDER BY h.position
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

    public async Task<AccountRecord?> FindAsync(string iban, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(iban);

        await using var command = _dataSource.CreateCommand(FindSql);
        command.Parameters.AddWithValue("iban", IbanFormat.Electronic(iban));

        string? lei = null;
        var holders = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lei = reader.IsDBNull(0) ? null : reader.GetString(0);
            holders.Add(reader.GetString(1));
        }

        // an account row with no holders can't be verified, so it reads the same as no account
        return holders.Count == 0 ? null : new AccountRecord(iban, holders, lei is null ? null : Lei.Parse(lei));
    }
}
