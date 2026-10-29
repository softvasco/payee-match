namespace PayeeMatch.Decisions;

/// <summary>Why a decision came out the way it did. Values are stable, so logs and reports can store them.</summary>
public enum ReasonCode
{
    /// <summary>The names only differ in case, accents, punctuation, titles or legal form.</summary>
    SameNameAfterNormalisation = 1,

    /// <summary>The same words in another order.</summary>
    WordOrderDiffers = 2,

    /// <summary>An initial stands for a word the other name spells out.</summary>
    InitialUsed = 3,

    /// <summary>A word is spelled differently but close enough to be a typo.</summary>
    SpellingDiffers = 4,

    /// <summary>One name has words the other leaves out, such as a middle name.</summary>
    WordsMissing = 5,

    /// <summary>Each name has a word the other doesn't, such as a different surname.</summary>
    WordsDiffer = 6,

    /// <summary>No full word of one name pairs with the other.</summary>
    NoWordInCommon = 7,

    /// <summary>One of the names is empty once normalised.</summary>
    NameEmpty = 8,

    /// <summary>Both names carry a legal form and they differ, such as Lda and SA.</summary>
    LegalFormDiffers = 9,

    /// <summary>The identification code sent is the one on record.</summary>
    IdentifierMatches = 10,

    /// <summary>The account holder has another identification code on record.</summary>
    IdentifierDiffers = 11,

    /// <summary>No identification code of that kind is on record for the account.</summary>
    IdentifierNotOnRecord = 12,
}
