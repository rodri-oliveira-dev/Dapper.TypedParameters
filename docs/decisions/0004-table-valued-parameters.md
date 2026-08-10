# 0004 - Table-valued parameter API

## Status

Accepted.

## Context

The library needs explicit SQL Server table-valued parameter support for Dapper
without adding schema introspection, POCO mapping, or provider-neutral
abstractions.

SQL Server TVPs require an existing user-defined table type and a parameter
`TypeName`. They are input-only for this package scope and do not use scalar
metadata such as `Size`, `Precision`, or `Scale`.

## Decision

- Add `SqlParam.TableValued(string typeName, DataTable value)`.
- Return a dedicated `SqlMapper.ICustomQueryParameter` implementation instead
  of extending `TypedSqlParameter`.
- Materialize with `SqlDbType.Structured`, `TypeName`, `Value`, and
  `ParameterDirection.Input`.
- Reject null, empty, and whitespace-only `typeName`.
- Reject a null `DataTable`.
- Support empty `DataTable` instances.
- Do not query SQL Server or validate the `DataTable` schema against the table
  type.
- Do not add POCO mapping in this step.
- Do not add an `IEnumerable<SqlDataRecord>` overload in this step.

## Consequences

The API remains small and additive. Callers are responsible for creating the
user-defined table type and building a `DataTable` whose columns match it.
Schema mismatches surface from `Microsoft.Data.SqlClient` or SQL Server during
command execution.

The `SqlDataRecord` path remains available for a future prompt if its public
provider type and behavior are covered by unit and integration tests for every
supported TFM.
