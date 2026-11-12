# 2. No names in logs, keyed hashes for account ids

Date: 2026-11-12

## Status

Accepted

## Context

Every verification request carries a payee name and an account identifier (an IBAN, sometimes with an LEI or another id). The name is personal data for natural persons, and the IBAN points to one person's account. The service handles a lot of these: a check runs before almost every credit transfer the payer's bank sends.

Logs and traces are the place where this kind of data leaks. They get shipped to a third-party platform, kept longer than anyone planned, and read by people who never need to see a customer's name. Once a name is in a log line, deleting it later means finding every copy.

On the other side, the service has to answer questions after the fact:
- A payer complains that a correct name came back as "no match". Support needs to find that request and see why.
- Someone asks whether one account is being probed with many different names.
- Operations needs latency and outcome counts per bank and per result.

None of those need the name itself.

## Decision

What goes in logs, traces and metrics:
- The request id (X-Request-ID) and the timestamp.
- The outcome (MTCH, CMTC, NMTC, NOAP) and the reason codes, which already say what differed ("legal form differs", "given name missing").
- The scores and the thresholds that applied.
- The account id as an HMAC-SHA256 with a key the host provides, never in clear.
- Latency and the requesting bank's BIC.

What never goes there: the payee name, the name held on the account, or any part of them. That includes exception messages, so the API turns domain exceptions into problem codes before anything is logged.

Names are not hashed either. A plain hash of a name can be reversed with a list of common names, and a keyed one would still only be useful to someone who could already guess the name. The reason codes carry what support needs.

The account id hash is keyed so it can't be reversed by hashing every IBAN of a bank, which is a small and well-structured space. Without the key, two log lines about the same account still match each other, which is enough to spot probing.

PayeeMatch doesn't store requests. Retention of the log and audit records is the host's decision, set by its own legal and complaints rules, and this repo doesn't pick a number for it.

## Consequences

- Support can find a request by its id and explain the outcome from the reason codes, without seeing either name.
- Rotating the HMAC key breaks correlation across the rotation. That is acceptable, and it limits how long a leaked key is useful.
- A test will send requests with known names through the API and fail if any of them shows up in captured logs or trace attributes. Without that, the rule is only a promise.
- Debugging a wrong match on real data needs the names, so it has to happen where they already are (the bank's own systems), not from the logs.
