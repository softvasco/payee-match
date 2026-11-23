using PayeeMatch.Accounts;
using PayeeMatch.Decisions;
using PayeeMatch.Identification;

namespace PayeeMatch.Api.Verification;

public static class PayeeVerificationEndpoint
{
    public const string Route = "/vop/v1/payee-verifications";
    public const string RequestIdHeader = "X-Request-ID";
    public const string RequestTimestampHeader = "X-Request-Timestamp";
    public const string ResponseTimestampHeader = "X-Response-Timestamp";

    public static IEndpointRouteBuilder MapPayeeVerification(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost(Route, VerifyAsync);
        return endpoints;
    }

    private static async Task<IResult> VerifyAsync(
        PayeeVerificationRequest request,
        HttpContext context,
        IAccountDirectory directory,
        DecisionEngine engine,
        TimeProvider time,
        CancellationToken cancellationToken)
    {
        var headers = context.Response.Headers;
        headers[ResponseTimestampHeader] = IsoTimestamp.Format(time.GetUtcNow());

        string? requestId = context.Request.Headers[RequestIdHeader];
        if (!Guid.TryParse(requestId, out _))
        {
            return Invalid($"{RequestIdHeader} must be a UUID.");
        }

        // the requester matches our answer to its call by this id, so it goes back exactly as sent
        headers[RequestIdHeader] = requestId;

        if (request.Party is not { } party)
        {
            return Invalid("party is required.");
        }

        // a malformed IBAN is the requester's mistake, not an account we don't hold, so it gets a 400 rather than NOAP
        if (!Iban.TryParse(request.PartyAccount?.Iban, out var parsed))
        {
            return Invalid("partyAccount.iban is not a valid IBAN.");
        }

        if ((InvalidBic(request.PartyAgent, "partyAgent") ?? InvalidBic(request.RequestingAgent, "requestingAgent")) is { } bicError)
        {
            return bicError;
        }

        var iban = parsed.Value;

        // name + IBAN or code + IBAN; the spec treats both together as a format error
        var hasCode = party.Identification is not null;
        if (party.Name is { } name && !hasCode)
        {
            return Results.Ok(VerifyName(name, await directory.FindAsync(iban, cancellationToken), engine));
        }

        if (party.Name is not null)
        {
            return Invalid("party has both a name and an identification; send one of them.");
        }

        // LEI is the only organisation id scheme this service checks
        if (party.Identification?.OrganisationId?.Lei is not { } leiText)
        {
            return Invalid("party needs a name or an organisation LEI; other identification schemes aren't supported.");
        }

        if (!Lei.TryParse(leiText, out var lei))
        {
            return Invalid("party.identification.organisationId.lei is not a valid LEI.");
        }

        return Results.Ok(VerifyCode(lei, await directory.FindAsync(iban, cancellationToken)));
    }

    private static IResult? InvalidBic(Agent? agent, string field) =>
        agent?.FinancialInstitutionId?.Bicfi is { } bic && !Bic.TryParse(bic, out _)
            ? Invalid($"{field}.financialInstitutionId.bicfi is not a valid BIC.")
            : null;

    private static IResult Invalid(string detail) =>
        Results.Problem(detail, statusCode: StatusCodes.Status400BadRequest, title: "The request is not valid.");

    private static PayeeVerificationResponse VerifyName(string name, AccountRecord? account, DecisionEngine engine)
    {
        if (account is null)
        {
            return new PayeeVerificationResponse(PartyNameMatch: MatchOutcome.NotApplicable.ToEpcCode());
        }

        var (holder, decision) = engine.Decide(name, account.Holders);

        // only a close match shows the payer the name on record, so a guess can't fish it out
        return decision.Outcome == MatchOutcome.CloseMatch
            ? new PayeeVerificationResponse(PartyNameMatch: decision.Outcome.ToEpcCode(), MatchedName: holder)
            : new PayeeVerificationResponse(PartyNameMatch: decision.Outcome.ToEpcCode());
    }

    private static PayeeVerificationResponse VerifyCode(Lei lei, AccountRecord? account)
    {
        var outcome = account is null ? MatchOutcome.NotApplicable : IdentificationCheck.Decide(lei, account.Lei).Outcome;
        return new PayeeVerificationResponse(PartyIdMatch: outcome.ToEpcCode());
    }
}
