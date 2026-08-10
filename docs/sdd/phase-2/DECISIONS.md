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
22. Table-valued parameters use a dedicated parameter type rather than extending the scalar `TypedSqlParameter` model.
23. Prompt 010 exposes only `SqlParam.TableValued(string typeName, DataTable value)`.
24. TVPs are materialized with `SqlDbType.Structured`, `TypeName`, `Value`, and `ParameterDirection.Input`.
25. Empty `DataTable` values are supported; null `DataTable` values are rejected.
26. The library does not infer TVP schema, validate table columns against SQL Server, or map POCOs.
27. No `IEnumerable<Microsoft.Data.SqlClient.Server.SqlDataRecord>` overload is added in prompt 010.
28. Package quality uses `Microsoft.SourceLink.GitHub`, SDK `.snupkg` symbols, deterministic builds, repository metadata, and CI-aware `ContinuousIntegrationBuild`.
29. Accidental public API changes are guarded by `Microsoft.CodeAnalysis.PublicApiAnalyzers` with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`.
30. Package API compatibility is validated with SDK package validation and strict compatible TFM checks; no published-package baseline is configured until a real published baseline exists.
31. Coverage uses the existing `coverlet.collector` and uploads Cobertura artifacts by suite and TFM; no threshold is set before measuring and agreeing on an evolution rule.
32. Benchmarks are isolated in a BenchmarkDotNet project and a manual workflow; full benchmark measurements are not part of pull request CI.
33. Dependency diagnostics use Central Package Management, NuGet audit, explicit vulnerability listing during validation, and GitHub dependency review on pull requests.
34. Restore locked mode is deferred until the repository makes a clear lock-file maintenance decision.
35. English is the canonical public documentation language.
36. `README.pt-BR.md` is a supported maintained translation of the main README.
37. Deeper technical documentation lives under `docs/`, with family-specific
    examples under `docs/examples/`.
38. Public documentation must not promise performance gains from explicit
    parameter metadata.
39. The package does not introspect database schema.
40. Consumers are responsible for choosing the SQL Server type that matches
    their schema or stored procedure contract.
