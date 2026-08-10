# 0002 - SQL Server temporal parameters

## Status

Accepted.

## Context

The library needs explicit factories for SQL Server temporal parameter types.
The requested public API uses `DateOnly`, `TimeOnly`, `DateTime`, and
`DateTimeOffset`.

`DateOnly` and `TimeOnly` are available in supported modern .NET TFMs, but not in
`netstandard2.0`.

## Decision

- Add temporal factories to `SqlParam` for `date`, `time`, `datetime`,
  `smalldatetime`, `datetime2`, and `datetimeoffset`.
- Use `DateOnly?`, `TimeOnly?`, `DateTime?`, and `DateTimeOffset?` as the public
  input types.
- Materialize `DateOnly` as `DateTime` at midnight and `TimeOnly` as `TimeSpan`
  when creating the underlying `SqlParameter`.
- Do not mutate `DateTime.Kind`, normalize to UTC, convert time zones, parse
  strings, or validate complete SQL Server date/time ranges.
- Validate temporal scale only for `time`, `datetime2`, and `datetimeoffset`,
  accepting `0` through `7`.
- Target `net8.0` and `net10.0`.

## Consequences

The package no longer targets `netstandard2.0`. This keeps the public API direct
and avoids compatibility shims before the first stable release.

