# 1. Explainable rule-based scoring instead of a trained model

Date: 2026-09-27

## Status

Accepted

## Context

A Verification of Payee check sits right before a payment is authorised. When it says "close match" or "no match", the payer may cancel, and a customer complaint or a regulator may later ask why. The EPC scheme gives guidelines for matching, not an algorithm, so every provider picks its own approach.

Two broad options:

1. A trained model (a classifier over name pairs, or embeddings with a similarity threshold).
2. Deterministic rules: normalise both names, compute string and token similarities, and decide with explicit thresholds.

Things that matter here:
- Every outcome has to be explainable in plain words: "legal form differs", "one given name missing", "typo in surname".
- The same input must always give the same answer, across versions unless a rule changed on purpose.
- There is no public labelled dataset of real payee names, and real ones can't be used because they are personal data.
- Bias: a model trained on mostly Western European names can quietly do worse on names from other cultures, and that is hard to see from outside.

## Decision

PayeeMatch uses deterministic, rule-based scoring. The pipeline is normalise, compare, decide:

- Normalisation is explicit and testable (diacritics, case, punctuation, titles, legal forms per country).
- Comparison uses well-known measures (Jaro-Winkler, Damerau-Levenshtein) and token rules (word order, initials, missing middle names).
- The decision maps scores to MTCH, CMTC or NMTC through configurable thresholds, and every result carries reason codes.

A model can come later as an extra signal behind the same interface, never as the only one.

## Consequences

- Each decision can be traced to the rules that fired, which is what an auditor or a complaints team needs.
- Thresholds are configuration, so a bank can tune its own balance between false alarms and missed mismatches, and the choice is visible.
- Rules need care per language and country. The synthetic evaluation set is how I keep them honest: precision and recall are measured on every change.
- Some cases a model might catch (nicknames, unusual transliterations) will need explicit rules or lists. I accept that in exchange for predictability.
