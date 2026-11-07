using PayeeMatch.Accounts;
using PayeeMatch.Identification;

namespace PayeeMatch.Core.Tests.Accounts;

/// <summary>What every account directory must do; each adapter runs these.</summary>
public abstract class AccountDirectoryContract
{
    protected const string JointIban = "PT50000201231234567890154";
    protected const string CompanyIban = "DE89370400440532013000";

    protected static readonly Lei CompanyLei = Lei.Parse("5493001KJTIIGC8Y1R12");

    private static readonly AccountRecord[] Accounts =
    [
        new(JointIban, ["Maria Silva", "João Silva"]),
        new(CompanyIban, ["Acme Trading GmbH"], CompanyLei),
    ];

    protected static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_joint_account_comes_back_with_its_holders_in_the_order_on_record()
    {
        var directory = await CreateAsync(Accounts);

        var account = await directory.FindAsync(JointIban, Token);

        Assert.NotNull(account);
        Assert.Equal(["Maria Silva", "João Silva"], account.Holders);
        Assert.Null(account.Lei);
    }

    [Fact]
    public async Task A_company_account_comes_back_with_its_lei()
    {
        var directory = await CreateAsync(Accounts);

        var account = await directory.FindAsync(CompanyIban, Token);

        Assert.Equal(CompanyLei, account?.Lei);
    }

    [Fact]
    public async Task The_iban_is_found_whatever_its_spacing_and_case()
    {
        var directory = await CreateAsync(Accounts);

        var account = await directory.FindAsync("de89 3704 0044 0532 0130 00", Token);

        Assert.Equal(CompanyIban, account?.Iban);
    }

    [Fact]
    public async Task An_iban_the_bank_does_not_hold_is_null()
    {
        var directory = await CreateAsync(Accounts);

        Assert.Null(await directory.FindAsync("FR7630006000011234567890189", Token));
    }

    protected abstract Task<IAccountDirectory> CreateAsync(IReadOnlyList<AccountRecord> accounts);
}
