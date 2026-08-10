# SDD phase 2

## Purpose

Phase 2 expands `Dapper.TypedParameters.SqlServer` beyond string parameters while preserving the small, explicit SQL Server API, the single package identity, and compatibility with `net8.0` and `net10.0`.

## Prompt order

Prompts must run in this order:

1. `006-numeric-parameters`
2. `007-binary-and-identifier-parameters`
3. `008-date-and-time-parameters`
4. `009-output-parameters`
5. `010-table-valued-parameters`
6. `011-package-quality`

Each prompt must run in a separate chat without relying on memory from previous chats.

## Execution rules

- Merge the current prompt before starting the next prompt.
- Keep `DECISIONS.md` and `STATUS.md` current during the phase.
- Create the prompt specification before implementation.
- Produce one semantic commit per prompt.
- Do not push, open pull requests, publish packages, create releases, or create tags automatically.
