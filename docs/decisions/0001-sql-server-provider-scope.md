# 0001 — SQL Server provider scope

## Status

Accepted.

## Context

The repository may host typed-parameter packages for additional database providers in the future. The first package must solve the SQL Server parameter-typing problem without introducing abstractions based on providers that do not yet exist.

## Decision

- The first package is named `Dapper.TypedParameters.SqlServer`.
- It supports `Microsoft.Data.SqlClient` only.
- `System.Data.SqlClient` is not supported.
- The public factory is named `SqlParam`.
- String extension methods such as `value.AsVarChar(11)` are not part of the initial API.
- SQL Server `max` types use explicit methods such as `VarCharMax` and `NVarCharMax`; callers do not pass `-1` directly.
- No provider-neutral `Core` or `Abstractions` package will be created until a second provider reveals genuinely shared concepts.

## Consequences

The initial implementation can use `SqlDbType` and `SqlParameter` directly and provide clear failures for incompatible ADO.NET providers. If another database package is created later, shared code will be extracted from proven duplication rather than predicted requirements.
