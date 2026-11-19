using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using PayeeMatch.Accounts;
using PayeeMatch.Api.Verification;
using PayeeMatch.Identification;

namespace PayeeMatch.Api.Tests;

public sealed class PayeeVerificationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private const string PersonIban = "DE89370400440532013000";
    private const string JointIban = "NL91ABNA0417164300";
    private const string CompanyIban = "GB82WEST12345698765432";
    private const string UnknownIban = "BE68539007547034";
    private const string CompanyLei = "5493001KJTIIGC8Y1R12";
    private const string OtherLei = "HWUPKR0MPOU8FGXBT394";

    private static readonly DateTimeOffset Now = new(2026, 11, 19, 20, 15, 30, 120, TimeSpan.Zero);

    private readonly HttpClient _client;

    public PayeeVerificationTests(WebApplicationFactory<Program> factory)
    {
        var directory = new InMemoryAccountDirectory(
        [
            new AccountRecord(PersonIban, ["Joao Manuel Silva"]),
            new AccountRecord(JointIban, ["Maria Santos", "Maria Fernanda Costa"]),
            new AccountRecord(CompanyIban, ["Padaria Central Lda"], Lei.Parse(CompanyLei)),
        ]);

        _client = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IAccountDirectory>(directory);
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(Now));
        })).CreateClient();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_same_name_is_a_match_without_the_name_on_record()
    {
        using var response = await VerifyAsync(NameRequest("Joao Manuel Silva", PersonIban));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("""{"partyNameMatch":"MTCH"}""", await response.Content.ReadAsStringAsync(Token));
    }

    [Fact]
    public async Task A_close_match_names_the_closest_holder_of_a_joint_account()
    {
        using var response = await VerifyAsync(NameRequest("Maria Costa", JointIban));

        var body = await BodyAsync(response);
        Assert.Equal("CMTC", (string?)body["partyNameMatch"]);
        Assert.Equal("Maria Fernanda Costa", (string?)body["matchedName"]);
    }

    [Fact]
    public async Task Another_name_is_no_match()
    {
        using var response = await VerifyAsync(NameRequest("Joao Santos", PersonIban));

        Assert.Equal("""{"partyNameMatch":"NMTC"}""", await response.Content.ReadAsStringAsync(Token));
    }

    [Fact]
    public async Task An_iban_the_bank_does_not_hold_is_not_applicable()
    {
        using var response = await VerifyAsync(NameRequest("Joao Manuel Silva", UnknownIban));

        Assert.Equal("""{"partyNameMatch":"NOAP"}""", await response.Content.ReadAsStringAsync(Token));
    }

    [Theory]
    [InlineData(CompanyLei, CompanyIban, "MTCH")]
    [InlineData(OtherLei, CompanyIban, "NMTC")]
    [InlineData(CompanyLei, PersonIban, "NOAP")]
    [InlineData(CompanyLei, UnknownIban, "NOAP")]
    public async Task An_lei_is_checked_against_the_one_on_record(string lei, string iban, string expected)
    {
        using var response = await VerifyAsync(LeiRequest(lei, iban));

        Assert.Equal($$"""{"partyIdMatch":"{{expected}}"}""", await response.Content.ReadAsStringAsync(Token));
    }

    [Fact]
    public async Task The_response_carries_the_request_id_and_its_own_timestamp()
    {
        const string requestId = "123e4567-e89b-12d3-a456-426614174000";

        using var response = await VerifyAsync(NameRequest("Joao Manuel Silva", PersonIban), requestId);

        Assert.Equal(requestId, Header(response, PayeeVerificationEndpoint.RequestIdHeader));
        Assert.Equal("2026-11-19T20:15:30.12Z", Header(response, PayeeVerificationEndpoint.ResponseTimestampHeader));
    }

    [Fact]
    public async Task A_request_without_a_request_id_is_rejected()
    {
        using var response = await VerifyAsync(NameRequest("Joao Manuel Silva", PersonIban), requestId: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_name_and_an_identification_code_together_are_rejected()
    {
        var request = LeiRequest(CompanyLei, CompanyIban);
        request["party"]!["name"] = "Padaria Central Lda";

        using var response = await VerifyAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose() => _client.Dispose();

    private static JsonObject NameRequest(string name, string iban) => Request(new JsonObject { ["name"] = name }, iban);

    private static JsonObject LeiRequest(string lei, string iban) => Request(
        new JsonObject { ["identification"] = new JsonObject { ["organisationId"] = new JsonObject { ["lei"] = lei } } },
        iban);

    // the shape of the example in section 5.1 of the spec
    private static JsonObject Request(JsonObject party, string iban) => new()
    {
        ["party"] = party,
        ["partyAccount"] = new JsonObject { ["iban"] = iban },
        ["partyAgent"] = new JsonObject { ["financialInstitutionId"] = new JsonObject { ["bicfi"] = "ABCDDEFFXXX" } },
        ["requestingAgent"] = new JsonObject { ["financialInstitutionId"] = new JsonObject { ["bicfi"] = "EFGHPTPLXXX" } },
    };

    private async Task<HttpResponseMessage> VerifyAsync(JsonObject body, string? requestId = "4f0b8c2e-6f1d-4a57-9f3e-1c2d3e4f5a6b")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(PayeeVerificationEndpoint.Route, UriKind.Relative))
        {
            Content = JsonContent.Create(body),
        };
        if (requestId is not null)
        {
            request.Headers.Add(PayeeVerificationEndpoint.RequestIdHeader, requestId);
        }

        request.Headers.Add(PayeeVerificationEndpoint.RequestTimestampHeader, "2026-11-19T20:15:30.1Z");
        return await _client.SendAsync(request, Token);
    }

    private static async Task<JsonNode> BodyAsync(HttpResponseMessage response) =>
        JsonNode.Parse(await response.Content.ReadAsStringAsync(Token))!;

    private static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.Single() : null;
}
