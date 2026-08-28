# 0006 - PostgreSQL provider scope

## Status

Accepted.

## Context

The first provider in this repository was SQL Server, as recorded in
[0001 - SQL Server provider scope](0001-sql-server-provider-scope.md). The
repository now needs a second provider for PostgreSQL using the official Npgsql
ADO.NET provider.

SQL Server and PostgreSQL both benefit from explicit parameter metadata at the
Dapper call site, but their provider APIs and database semantics differ
materially.

## Decision

- Add a provider-specific PostgreSQL package with NuGet Package ID
  `TypedParameters.Dapper.PostgreSql`.
- The PostgreSQL assembly is `Dapper.TypedParameters.PostgreSql.dll`.
- The PostgreSQL root namespace and public namespace are
  `Dapper.TypedParameters.PostgreSql`.
- Keep SQL Server and PostgreSQL as independent provider projects and packages.
- The SQL Server provider continues to use `Microsoft.Data.SqlClient` and
  `SqlDbType` directly.
- The PostgreSQL provider uses `Npgsql` and `NpgsqlDbType` directly.
- Do not create `TypedDbParameter`, `DbParam`, `IDbTypedParameter`, a shared
  base class, a provider-neutral `Core` package, or another cross-provider
  abstraction solely to reduce duplication.
- Prefer a small amount of explicit provider-specific duplication over an
  abstraction that hides database behavior.
- Reconsider shared code only after both providers demonstrate a genuinely
  identical responsibility.

## Consequences

The repository can host multiple NuGet packages without implying a common
runtime contract between them. PostgreSQL-specific differences such as JSON vs
JSONB, timestamp semantics, arrays, ranges, multiranges, enums, composites, and
the absence of a SQL Server-style table-valued parameter equivalent remain
visible in the provider design instead of being flattened behind a premature
abstraction.

## Phase 3 parameter metadata decisions

PostgreSQL parameter metadata must describe behavior that is observable through
Npgsql and PostgreSQL, not behavior copied from `SqlParameter`.

The PostgreSQL provider exposes `VarChar(string? value)` and
`Char(string? value)` without a size argument. Integration tests with
`NpgsqlDbType.Varchar`/`NpgsqlDbType.Char` and `NpgsqlParameter.Size` showed
that PostgreSQL observes the backend type as `character varying` or
`character`, but not a `varchar(n)` or `char(n)` typmod. Values longer than
`Size` are truncated before they reach PostgreSQL. A public
`VarChar(value, size)` or `Char(value, size)` API would therefore be misleading:
it could look like a server-side contract while actually enabling client-side
payload truncation.

The PostgreSQL provider exposes `Numeric(decimal? value)` without precision or
scale arguments. Integration tests with `NpgsqlDbType.Numeric`,
`NpgsqlParameter.Precision`, and `NpgsqlParameter.Scale` showed PostgreSQL
observing `numeric`; a value exceeding the declared metadata round-tripped
without provider rounding, truncation, or server validation. Precision and
scale are not exposed until a future use case can describe their effect without
implying PostgreSQL `numeric(p, s)` typmod semantics.

`Json(string? value)` and `Jsonb(string? value)` accept caller-provided JSON
text only. The provider does not serialize POCOs, use `JsonSerializer`, handle
`JsonDocument` specially, or configure global Npgsql JSON mapping.

Temporal factories use provider-specific timestamp semantics:

- `Timestamp(DateTime? value)` represents PostgreSQL
  `timestamp without time zone` / wall-clock time. It accepts
  `DateTimeKind.Local` and `DateTimeKind.Unspecified`, and rejects
  `DateTimeKind.Utc`.
- `TimestampTz(DateTime? value)` represents PostgreSQL
  `timestamp with time zone` as a UTC instant. It accepts only
  `DateTimeKind.Utc` and does not convert local or unspecified values.
- `Interval(TimeSpan? value)` is supported for the first version, with the
  documented limitation that PostgreSQL intervals can contain month and year
  components that `TimeSpan` cannot represent.
