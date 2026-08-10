# 007 - Binary and identifier parameters

## Context

Phase 2 already added explicit SQL Server numeric and boolean parameters after the string API from phase 1. The next expansion covers binary payloads and SQL Server identifiers while preserving the small, explicit public API, the `Microsoft.Data.SqlClient` provider scope, and equivalent behavior for `net8.0` and `net10.0`.

## Problem

Consumers can declare string and numeric SQL Server parameter metadata explicitly, but `uniqueidentifier`, `binary`, and `varbinary` values still depend on provider inference unless callers create `SqlParameter` instances manually. Binary parameters also need explicit size metadata without changing the supplied byte array.

## Proposed API

```csharp
SqlParam.UniqueIdentifier(Guid? value);
SqlParam.Binary(byte[]? value, int size);
SqlParam.VarBinary(byte[]? value, int size);
SqlParam.VarBinaryMax(byte[]? value);
```

## Size limits

- `binary`: `size` must be from 1 to 8,000.
- `varbinary`: `size` must be from 1 to 8,000.
- `varbinary(max)`: uses `Size = -1`.
- Factories with `size` reject invalid values with `ArgumentOutOfRangeException` and `size` as the parameter name.

## Array handling

The library stores the supplied `byte[]` reference as the parameter value. It does not copy arrays without a proven need, does not validate `value.Length <= size`, and does not truncate content. Only `null` is converted to `DBNull.Value` when Dapper materializes the `SqlParameter`; empty arrays remain empty arrays.

## binary behavior

`SqlParam.Binary` declares `SqlDbType.Binary` and the supplied fixed size. It does not pad or truncate in library code. SQL Server and `Microsoft.Data.SqlClient` remain responsible for fixed-length binary semantics during execution.

## varbinary behavior

`SqlParam.VarBinary` declares `SqlDbType.VarBinary` and the supplied size. It preserves empty arrays and non-null array references exactly as supplied before provider execution.

## max types

`SqlParam.VarBinaryMax` declares `SqlDbType.VarBinary` with `Size = -1`, matching SQL Server `varbinary(max)`. There is no `BinaryMax` because SQL Server fixed-length `binary` has no `max` form.

## Guid handling

`SqlParam.UniqueIdentifier` accepts `Guid?`, including `Guid.Empty`, declares `SqlDbType.UniqueIdentifier`, and converts only `null` to `DBNull.Value` during materialization.

## Non-objectives

- No `image` factory.
- No `rowversion` factory.
- No `timestamp` factory.
- No `filestream` API.
- No schema inspection or extra database queries.
- No type inference from value length.
- No copying, padding, truncation, or mutation of byte arrays in library code.

`rowversion` and `timestamp` are not common input parameter types and remain outside this prompt scope.

## Tests

Unit tests cover `Guid`, `Guid.Empty`, nullable `Guid` null, normal arrays, empty arrays, null arrays, minimum and maximum sizes, zero size, negative size, sizes greater than 8,000, `varbinary(max)`, `SqlCommand` materialization, and reuse of an existing `SqlParameter`.

Integration tests cover `uniqueidentifier`, `binary(n)`, `varbinary(n)`, `varbinary(max)`, null values, empty arrays, exact byte round trips, `WHERE`, `INSERT`, async Dapper execution, and validation for both `net8.0` and `net10.0`.

## Acceptance criteria

- Public API factories are implemented with XML documentation.
- `UniqueIdentifier` materializes as `SqlDbType.UniqueIdentifier`.
- `Binary` materializes as `SqlDbType.Binary` with declared size.
- `VarBinary` and `VarBinaryMax` materialize as `SqlDbType.VarBinary` with declared size.
- `varbinary(max)` uses `Size = -1`.
- Invalid bounded binary sizes throw `ArgumentOutOfRangeException`.
- Empty arrays are not converted to `DBNull.Value`.
- The library does not validate value length against declared size or truncate arrays.
- Unit and integration tests pass for `net8.0` and `net10.0`, or integration blockers are documented.
- README, CHANGELOG, decisions, status, and this spec are updated.
- Exactly one commit is created: `feat: add binary and identifier parameters`.

## Risks

- SQL Server fixed-length `binary(n)` can pad shorter values; tests should use exact-length input when asserting exact binary round trips.
- Binary values larger than the declared non-max size may fail at provider or SQL Server execution time; the library intentionally does not pre-validate that condition.
- Integration tests depend on Docker and SQL Server startup.
- Reused `SqlParameter` instances may retain metadata not declared by a new typed parameter; the implementation only sets declared metadata, preserving current reuse behavior.

## Validation commands

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
git diff --check
```

## Planned commit

```text
feat: add binary and identifier parameters
```

## Validation results

- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- Unit tests `net8.0`: passed; 87 passed, 0 failed, 0 skipped.
- Unit tests `net10.0`: passed; 87 passed, 0 failed, 0 skipped.
- Integration tests `net8.0`: passed; 20 passed, 0 failed, 0 skipped.
- Integration tests `net10.0`: passed; 20 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed; generated `Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`.
- Docker was available; no integration blocker was recorded.
