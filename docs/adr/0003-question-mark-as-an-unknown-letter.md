# 3. A ? inside a word is one unknown letter

Date: 2026-11-17

## Status

Accepted

## Context

A Verification of Payee request carries the name the payer typed, but some PSPs only pass on characters from the SEPA set (basic Latin letters, digits and a few punctuation marks) and replace anything else with `?`. "João Conceição" arrives as "Jo?o Concei??o", and "Øster" as "?ster" (issue #41).

The normaliser treats `?` as punctuation and splits on it, so "Jo?o" becomes the two tokens "jo" and "o". Those score badly against "joao", and a name the payer typed correctly can come back as NMTC. The ABE/EBA paper on VoP matching lists disallowed characters as one of the common causes of false mismatches.

Options:

1. Keep splitting. Simple, but it punishes the payer for what their bank did.
2. Drop the `?`. "Jo?o" becomes "joo", one deletion from "joao", which Jaro-Winkler forgives. It also turns "M?ller" into "mller", and short words lose too much.
3. Keep the `?` inside the word as a placeholder that stands for exactly one letter of the other name.

## Decision

Option 3.

- The normaliser keeps `?` as part of a word when it sits next to a letter or digit of that word. A `?` on its own ("Silva ?") still separates words and leaves nothing behind.
- When two tokens have the same length and agree on every position where neither has a `?`, they pair as a new kind, `Placeholder`. Each `?` costs 0.05 off a pair score of 1, so "Jo?o Silva" against "João Silva" scores 0.975 and stays a match with the default thresholds, while a word that is all `?` is worth little.
- A placeholder pair adds the reason code `UnknownLetter`, not `SpellingDiffers`, so a company name with one isn't capped at close match the way a typo is.
- Tokens of different lengths fall back to Jaro-Winkler with `?` as an ordinary character. That covers the rare case where one replaced letter stood for two after folding ("Stra?e" against "strasse").

## Consequences

- A `?` can stand for any letter, so "M?ller" pairs with "Moller" as well as "Muller". The penalty and the reason code make that visible; a bank that wants more caution can raise its match threshold.
- Property tests check that replacing any one letter of a word with `?` never scores lower than splitting did, and never higher than the exact word.
- A real question mark typed by the payer inside a word is treated the same way. That is rare in names and was already noise before.
