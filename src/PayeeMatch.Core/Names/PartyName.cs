namespace PayeeMatch.Names;

/// <summary>A normalised name with titles and the legal form taken out. The legal form is kept apart because a mismatch there still matters.</summary>
public sealed record PartyName(NormalisedName Core, string? LegalForm)
{
    public bool HasLegalForm => LegalForm is not null;
}
