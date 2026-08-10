# 0003 - Output parameter API

## Status

Accepted.

## Context

The library needs to support SQL Server `Output` and `InputOutput` parameters
for every scalar `SqlParam` factory without breaking existing input parameter
call sites.

The API must avoid ambiguous optional parameters, preserve existing factories,
retain the materialized `SqlParameter` internally, and provide a clear value
retrieval API after Dapper command execution.

## Decision

- Keep all existing factories unchanged and defaulting to `Input`.
- Add fluent `TypedSqlParameter.AsOutput()` and
  `TypedSqlParameter.AsInputOutput()` methods.
- Add `TypedSqlParameter.OutputValue` and `TypedSqlParameter.GetValue<T>()`.
- Retain the latest materialized `SqlParameter` internally for output reads.
- Normalize output `DBNull.Value` to `null`.
- Throw for non-nullable value type reads of database null.
- Use CLR casting rules only; do not perform silent conversions.
- Document that output parameter instances are not thread-safe for concurrent
  use in different commands.

## Consequences

The public surface stays compact and source-compatible. Callers must retain the
same fluent output instance they pass to Dapper and read values only after
command execution completes.
