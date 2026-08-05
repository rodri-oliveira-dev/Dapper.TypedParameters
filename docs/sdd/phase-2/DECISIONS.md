# Phase 2 decisions

## Accepted

1. A single package continues to serve both `net8.0` and `net10.0`.
2. The public API must remain equivalent in both TFMs.
3. All parameter types remain SQL Server-specific.
4. `Microsoft.Data.SqlClient` remains the only supported provider.
5. Dependency versions remain centralized through Central Package Management.
6. Factory methods must not infer SQL Server types from values.
7. `null` continues to be materialized as `DBNull.Value`.
8. No provider-neutral abstraction will be created in this phase.
9. SQL Server `numeric` is a synonym of `decimal`; the phase will expose `SqlParam.Decimal` and will not add a separate `SqlParam.Numeric` factory.
10. Generic numeric overloads such as `SqlParam.Number<T>` are not part of the API.
11. Binary factories preserve the supplied `byte[]` reference and do not validate value length against declared size.
12. `varbinary(max)` is represented by `SqlDbType.VarBinary` with `Size = -1`.
13. `image`, `rowversion`, `timestamp`, and `filestream` are outside the binary parameter scope; `rowversion` and `timestamp` are not common input parameter types.
14. Public temporal factories use `DateOnly`, `TimeOnly`, `DateTime`, and `DateTimeOffset`.
15. `DateOnly` is materialized as `DateTime` at midnight and `TimeOnly` as `TimeSpan`.
16. Temporal values are not timezone-normalized, parsed, rounded, or range-validated by the library.
17. `time`, `datetime2`, and `datetimeoffset` accept scale values from `0` to `7`, defaulting to `7`.
18. Output support uses fluent `AsOutput()` and `AsInputOutput()` methods on `TypedSqlParameter`; factories remain unchanged and default to `Input`.
19. Output values are read through `OutputValue` or `GetValue<T>()`; the mutable `SqlParameter` is retained internally but not exposed publicly.
20. `DBNull.Value` output is normalized to `null`; non-nullable value type reads throw instead of returning `default`.
21. Output parameter instances may be reused non-concurrently, but must not be shared concurrently across commands.
