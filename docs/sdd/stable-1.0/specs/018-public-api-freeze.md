# 018 - Public API Freeze

## Context

Prompt 017 validated public NuGet consumption of
`TypedParameters.Dapper.SqlServer 0.1.0-preview.1` from NuGet.org for both
`net8.0` and `net10.0`.

Prompt 018 performs the final deliberate review of the public API before the
first stable release candidate.

## Goal

Answer:

```text
Would we be comfortable supporting this public contract throughout 1.x?
```

The review must freeze the candidate 1.0 public API or explicitly block the
1.0 API freeze.

## Scope

- `SqlParam` factories.
- `TypedSqlParameter`.
- `TableValuedSqlParameter`.
- Public API baseline files.
- Nullability annotations, runtime behavior, XML documentation, and tests.
- Provider boundary for `Microsoft.Data.SqlClient`.
- Documentation and changelog consistency.

## Constraints

- Do not add new features for convenience.
- Keep changes small and limited to contract stabilization.
- Do not introduce provider-neutral abstractions.
- Do not add `System.Data.SqlClient`.
- Do not publish, tag, push, or create releases.
- Record any intentional breaking change as happening before `1.0.0`.

## Acceptance Criteria

- Complete API inventory is recorded in
  `docs/sdd/stable-1.0/reports/018-public-api-review.md`.
- Each relevant decision is classified as `KEEP`, `CHANGE BEFORE 1.0`, or
  `DEFER`.
- Public API baseline files deliberately represent the 1.0 candidate contract.
- Documentation does not contradict the runtime contract.
- Full validation is executed or blockers are recorded.
- `DECISIONS.md` and `STATUS.md` record the freeze outcome.

## Planned Commit

```text
refactor: stabilize public API for 1.0
```
