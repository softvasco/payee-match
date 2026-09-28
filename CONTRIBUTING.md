# Contributing

Thanks for looking. Issues and pull requests are welcome, and tricky name pairs that the matcher gets wrong are especially useful.

## Before you start

For anything bigger than a typo, open an issue first so we can agree on the approach. Design choices that are hard to reverse get an ADR in `docs/adr/`.

## Working on the code

```bash
dotnet build
dotnet test
```

- .NET 10 SDK. Warnings are errors, so the build has to be clean.
- Every matching rule needs tests that show a case it fixes and a case it must not change.
- Every outcome must keep a reason code. If a change makes a decision harder to explain, it needs a good argument in the PR.
- Never use real people's names or account details in tests, samples or the evaluation set. Made-up names only.

## Commits and pull requests

Conventional Commits: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`, with a scope when it helps (`feat(names): ...`). CI must be green before merge.
