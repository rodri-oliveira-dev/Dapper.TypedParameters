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
