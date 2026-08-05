# 008 - Temporal parameters

## Context

Prompt 008 adds explicit SQL Server temporal parameter factories to
`Dapper.TypedParameters.SqlServer`.

The requested handoff expected prompt 007 to be integrated into `main` and
referenced phase-2 files for prompts 006 and 007. Those files were not present in
the accessible repository state. The implementation therefore records the
temporal contract independently and keeps the change scoped to the requested
factories, tests, documentation, and TFM compatibility.

## Public .NET types

The public API accepts the following nullable CLR temporal types:

- `DateOnly?` for SQL Server `date`.
- `TimeOnly?` for SQL Server `time`.
- `DateTime?` for SQL Server `datetime`, `smalldatetime`, and `datetime2`.
- `DateTimeOffset?` for SQL Server `datetimeoffset`.

Factory names remain:

- `SqlParam.Date(DateOnly? value)`.
- `SqlParam.Time(TimeOnly? value, byte scale = 7)`.
- `SqlParam.DateTime(DateTime? value)`.
- `SqlParam.SmallDateTime(DateTime? value)`.
- `SqlParam.DateTime2(DateTime? value, byte scale = 7)`.
- `SqlParam.DateTimeOffset(DateTimeOffset? value, byte scale = 7)`.

The `DateTime` and `DateTimeOffset` factory names intentionally match SQL Server
type names. No namespace conflict required renaming because callers access them
as static methods on `SqlParam`.

## Internal conversions

The API keeps modern public types where they express intent best. During Dapper
materialization into `SqlParameter`:

- `DateOnly` is converted to `DateTime` at `TimeOnly.MinValue`.
- `TimeOnly` is converted to `TimeSpan`.
- `DateTime` is assigned unchanged.
- `DateTimeOffset` is assigned unchanged.
- `null` is assigned as `DBNull.Value`.

These conversions match the traditional ADO.NET SQL Server mappings for `date`
and `time` without changing the represented date or time value.

## Precision and scale

`time`, `datetime2`, and `datetimeoffset` expose SQL Server scale as `byte` and
default to `7`.

Valid scale values are `0` through `7`. Values greater than `7`, including values
created by unchecked casts from negative signed integers, throw
`ArgumentOutOfRangeException`.

`date`, `datetime`, and `smalldatetime` do not expose scale.

No factory performs manual rounding. SQL Server and `Microsoft.Data.SqlClient`
own storage precision behavior for:

- `time(n)`.
- `datetime`.
- `smalldatetime`.
- `datetime2(n)`.
- `datetimeoffset(n)`.

## DateOnly behavior

`DateOnly` represents only the calendar date. It is materialized as a `DateTime`
with midnight time for `SqlDbType.Date`. No time zone, offset, or kind semantics
are introduced by the public API.

## TimeOnly behavior

`TimeOnly` represents only the time of day. It is materialized as `TimeSpan` for
`SqlDbType.Time`. The configured `Scale` is passed to `SqlParameter.Scale`.

## DateTime behavior

`DateTime` values are passed unchanged for `datetime`, `smalldatetime`, and
`datetime2`. The library does not mutate `DateTime.Kind`, normalize to UTC,
parse strings, or perform timezone conversion.

## DateTimeOffset behavior

`DateTimeOffset` values are passed unchanged for `datetimeoffset`. The library
does not offer `datetimeoffset` from `DateTime`, does not normalize offsets, and
does not convert to UTC.

## Validations

- `scale` for `time`, `datetime2`, and `datetimeoffset` must be between `0` and
  `7`.
- Temporal factories do not validate the full SQL Server date/time ranges.
- Temporal factories do not configure `Size`.

## TFM compatibility

The package targets `net8.0` and `net10.0`. The implementation uses APIs
available in `net8.0` and does not use .NET 10-only APIs.

`netstandard2.0` is not compatible with the required public API because
`DateOnly` and `TimeOnly` are unavailable there.

## Tests

Unit tests cover:

- all temporal factories;
- null values;
- default, minimum, and maximum scale;
- invalid scale;
- `SqlDbType`;
- public value preservation;
- materialized `SqlParameter.Value`;
- absence of `Size`;
- `Scale` configuration.

Integration tests cover Dapper round-trips for:

- `date`;
- `time(0)`;
- `time(7)`;
- `datetime`;
- `smalldatetime`;
- `datetime2(0)`;
- `datetime2(7)`;
- `datetimeoffset(0)`;
- `datetimeoffset(7)`;
- null values;
- fractional values;
- positive and negative offsets;
- asynchronous execution.

The integration test runs only when
`DAPPER_TYPEDPARAMETERS_SQLSERVER_CONNECTION_STRING` is configured.

## Risks

- SQL Server precision behavior differs by temporal type. Tests compare with
  tolerances or rounded expectations that match the SQL type precision.
- `DateOnly` and `TimeOnly` required removing `netstandard2.0`; this is a
  deliberate compatibility decision before the first stable release.
- The prompt 006/007 handoff was unavailable in the accessible `main`, so this
  spec records prompt 008 independently instead of extending missing phase-2
  specs.

