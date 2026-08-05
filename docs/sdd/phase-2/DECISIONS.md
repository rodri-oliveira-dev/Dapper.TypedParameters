# Phase 2 decisions

## Temporal parameters

- Public temporal factories use `DateOnly`, `TimeOnly`, `DateTime`, and
  `DateTimeOffset`.
- `DateOnly` is materialized as `DateTime` at midnight when adding the
  `SqlParameter`.
- `TimeOnly` is materialized as `TimeSpan` when adding the `SqlParameter`.
- `DateTime` and `DateTimeOffset` values are materialized without timezone
  conversion, UTC normalization, parsing, or `DateTime.Kind` mutation.
- `time`, `datetime2`, and `datetimeoffset` accept `Scale` values from `0` to
  `7`, defaulting to `7`.
- Temporal parameters do not configure `Size`.
- SQL Server range enforcement remains delegated to SQL Server and
  `Microsoft.Data.SqlClient`.
- The package now targets `net8.0` and `net10.0`; `netstandard2.0` was removed
  because the public API requires `DateOnly` and `TimeOnly`.

