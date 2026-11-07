using Npgsql;
using PayeeMatch.Accounts;
using PayeeMatch.Core.Tests.Accounts;
using Testcontainers.PostgreSql;

namespace PayeeMatch.PostgreSql.Tests;

public sealed class PostgreSqlServer : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        DataSource = NpgsqlDataSource.Create(_container.GetConnectionString());
    }

    public async ValueTask DisposeAsync()
    {
        await DataSource.DisposeAsync();
        await _container.DisposeAsync();
    }
}

public sealed class PostgreSqlAccountDirectoryTests(PostgreSqlServer server) : AccountDirectoryContract, IClassFixture<PostgreSqlServer>
{
    [Fact]
    public async Task An_account_row_without_holders_reads_as_no_account()
    {
        var directory = await CreateAsync([]);
        await Execute("INSERT INTO accounts (iban) VALUES ('NL91ABNA0417164300')");

        Assert.Null(await directory.FindAsync("NL91ABNA0417164300", Token));
    }

    protected override async Task<IAccountDirectory> CreateAsync(IReadOnlyList<AccountRecord> accounts)
    {
        await Execute($"DROP TABLE IF EXISTS account_holders, accounts; {PostgreSqlAccountDirectory.Schema}");
        foreach (var account in accounts)
        {
            await using var batch = server.DataSource.CreateBatch();
            batch.BatchCommands.Add(Command("INSERT INTO accounts (iban, lei) VALUES ($1, $2)", account.Iban, account.Lei?.Value));
            for (var i = 0; i < account.Holders.Count; i++)
            {
                batch.BatchCommands.Add(
                    Command("INSERT INTO account_holders (iban, position, name) VALUES ($1, $2, $3)", account.Iban, (short)i, account.Holders[i]));
            }

            await batch.ExecuteNonQueryAsync(Token);
        }

        return new PostgreSqlAccountDirectory(server.DataSource);
    }

    private static NpgsqlBatchCommand Command(string sql, params object?[] values)
    {
        var command = new NpgsqlBatchCommand(sql);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value ?? DBNull.Value });
        }

        return command;
    }

    private async Task Execute(string sql)
    {
        await using var command = server.DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(Token);
    }
}
