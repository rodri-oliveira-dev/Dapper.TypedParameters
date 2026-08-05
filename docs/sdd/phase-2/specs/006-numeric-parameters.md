# 006 - Numeric parameters

## Context

Phase 1 delivered explicit SQL Server string parameters for Dapper in a single package targeting `net8.0` and `net10.0`. Phase 2 starts by adding SQL Server numeric and boolean parameters while preserving the existing string API and provider scope.

## Problem

Consumers can currently declare string metadata explicitly, but numeric SQL Server parameters still depend on provider inference unless callers create `SqlParameter` instances manually. Decimal parameters also need explicit precision and scale at the call site.

## Objectives

- Add explicit factories for SQL Server boolean and numeric parameter types.
- Preserve the existing public string API.
- Keep behavior equivalent between `net8.0` and `net10.0`.
- Preserve `Microsoft.Data.SqlClient` as the only provider.
- Preserve `DBNull.Value` materialization for `null`.
- Configure precision and scale only when explicitly declared.
- Avoid configuring `Size` for numeric parameters.

## Non-objectives

- No provider-neutral abstractions.
- No `System.Data.SqlClient` support.
- No schema inspection or extra database queries.
- No type inference from values.
- No generic numeric factory.
- No separate `Numeric` factory because SQL Server `numeric` is a synonym of `decimal`.
- No rounding or conversion performed by the library.

## Proposed API

```csharp
SqlParam.Bit(bool? value);
SqlParam.TinyInt(byte? value);
SqlParam.SmallInt(short? value);
SqlParam.Int(int? value);
SqlParam.BigInt(long? value);
SqlParam.Real(float? value);
SqlParam.Float(double? value);
SqlParam.Decimal(decimal? value, byte precision, byte scale);
SqlParam.Money(decimal? value);
SqlParam.SmallMoney(decimal? value);
```

## Accepted .NET types

| Factory | .NET type |
| --- | --- |
| `Bit` | `bool?` |
| `TinyInt` | `byte?` |
| `SmallInt` | `short?` |
| `Int` | `int?` |
| `BigInt` | `long?` |
| `Real` | `float?` |
| `Float` | `double?` |
| `Decimal` | `decimal?` |
| `Money` | `decimal?` |
| `SmallMoney` | `decimal?` |

## SqlParameter metadata

Each factory returns `TypedSqlParameter` with the declared `SqlDbType`. `Decimal` also carries declared `Precision` and `Scale`. Numeric factories do not declare `Size`.

## Precision and scale rules

- `precision` must be from 1 to 38.
- `scale` must be from 0 to `precision`.
- `Decimal` stores precision and scale as immutable optional metadata.
- `Money` and `SmallMoney` do not declare precision or scale; SQL Server/provider semantics apply.
- The library does not round decimal values.

## Validations

`SqlParam.Decimal` throws `ArgumentOutOfRangeException` with `precision` or `scale` as the parameter name when arguments are outside the supported SQL Server range. Other numeric factories do not need range validation beyond their .NET nullable type.

## TFM compatibility

The implementation uses shared `net8.0`-compatible C# and ADO.NET APIs. No TFM-specific code or conditional references are expected.

## Internal design

Extend `TypedSqlParameter` with optional immutable `Precision` and `Scale` properties. `AddParameter` will configure `SqlDbType`, then set `Size`, `Precision`, and `Scale` only when their metadata is declared.

## Impact on TypedSqlParameter

Existing string behavior remains unchanged. Existing constructor call sites continue using optional `size`. Public properties add nullable precision and scale metadata without removing or changing existing members.

## Unit tests

Unit tests must cover all factories, non-null values, null values, `SqlDbType`, precision, scale, decimal validation boundaries, scale greater than precision, precision zero, precision greater than 38, parameter reuse, absence of numeric size, and API equivalence between `net8.0` and `net10.0`.

## Integration tests

Integration tests must use the existing SQL Server container fixture and cover round trips for numeric families, `decimal(18,2)`, `decimal(38,18)`, null, negative values, representative limits, Dapper anonymous objects, async execution, `INSERT`, `SELECT`, and `WHERE`. Use `SQL_VARIANT_PROPERTY` for base type, precision, and scale when practical.

## Acceptance criteria

- Public API factories are implemented with XML documentation.
- `TypedSqlParameter` exposes immutable optional precision and scale metadata.
- Numeric parameters materialize with the expected `SqlDbType`.
- Decimal precision and scale are validated and configured.
- Numeric parameters do not configure `Size`.
- Unit and integration tests pass for `net8.0` and `net10.0`, or integration blockers are documented.
- README, CHANGELOG, decisions, status, and spec are updated.
- Exactly one commit is created: `feat: add SQL Server numeric parameters`.

## Risks

- SQL Server decimal metadata can be sensitive to casts and expression context.
- Integration tests depend on Docker and SQL Server startup.
- Reused `SqlParameter` instances may contain previous metadata not declared by a new typed parameter; the implementation only sets declared metadata, preserving current reuse behavior.

## Expected files

- `src/Dapper.TypedParameters.SqlServer/SqlParam.cs`
- `src/Dapper.TypedParameters.SqlServer/TypedSqlParameter.cs`
- `tests/Dapper.TypedParameters.SqlServer.Tests/SqlParamTests.cs`
- `tests/Dapper.TypedParameters.SqlServer.Tests/TypedSqlParameterTests.cs`
- `tests/Dapper.TypedParameters.SqlServer.IntegrationTests/DapperSqlServerParameterTests.cs`
- `README.md`
- `CHANGELOG.md`
- `docs/sdd/phase-2/README.md`
- `docs/sdd/phase-2/DECISIONS.md`
- `docs/sdd/phase-2/STATUS.md`
- `docs/sdd/phase-2/specs/006-numeric-parameters.md`

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
feat: add SQL Server numeric parameters
```

## Validation results

- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- Unit tests `net8.0`: passed; 60 passed, 0 failed, 0 skipped.
- Unit tests `net10.0`: passed; 60 passed, 0 failed, 0 skipped.
- Integration tests `net8.0`: passed; 14 passed, 0 failed, 0 skipped.
- Integration tests `net10.0`: passed; 14 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed; generated `Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`.
- Docker was available; no integration blocker was recorded.
