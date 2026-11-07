using PayeeMatch.Accounts;

namespace PayeeMatch.Core.Tests.Accounts;

public sealed class InMemoryAccountDirectoryTests : AccountDirectoryContract
{
    [Fact]
    public void The_same_iban_twice_is_refused_even_spaced_differently()
    {
        AccountRecord[] accounts = [new(CompanyIban, ["Acme Trading GmbH"]), new("DE89 3704 0044 0532 0130 00", ["Acme"])];

        Assert.Throws<ArgumentException>(() => new InMemoryAccountDirectory(accounts));
    }

    [Fact]
    public void An_account_needs_a_named_holder()
    {
        Assert.Throws<ArgumentException>(() => new AccountRecord(CompanyIban, []));
        Assert.Throws<ArgumentException>(() => new AccountRecord(CompanyIban, ["Acme", " "]));
    }

    protected override Task<IAccountDirectory> CreateAsync(IReadOnlyList<AccountRecord> accounts) =>
        Task.FromResult<IAccountDirectory>(new InMemoryAccountDirectory(accounts));
}
