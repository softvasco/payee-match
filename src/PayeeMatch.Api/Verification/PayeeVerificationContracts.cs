using System.Text.Json.Serialization;

namespace PayeeMatch.Api.Verification;

// names and nesting follow the EPC VOP API spec (EPC103-24 v1.1.1, sections 4.1 and 4.2)
public sealed record PayeeVerificationRequest(
    Party? Party,
    PartyAccount? PartyAccount,
    Agent? PartyAgent,
    Agent? RequestingAgent,
    IReadOnlyList<string>? UnstructuredRemittanceInformation);

/// <summary>Either a name or an identification code, never both.</summary>
public sealed record Party(string? Name, PartyIdentification? Identification);

public sealed record PartyIdentification(OrganisationIdentification? OrganisationId);

public sealed record OrganisationIdentification(string? Lei);

public sealed record PartyAccount(string? Iban);

public sealed record Agent(FinancialInstitutionId? FinancialInstitutionId);

public sealed record FinancialInstitutionId(string? Bicfi);

/// <summary>partyNameMatch or partyIdMatch, plus matchedName only on a close match.</summary>
public sealed record PayeeVerificationResponse(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PartyNameMatch = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PartyIdMatch = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MatchedName = null);
