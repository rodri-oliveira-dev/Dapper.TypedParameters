# 018 - Public API Review

## Public API inventory

Public namespace:

- `Dapper.TypedParameters.SqlServer`.

Public types:

- `SqlParam`: public static factory class.
- `TypedSqlParameter`: public sealed scalar parameter type implementing
  `Dapper.SqlMapper.ICustomQueryParameter`; public construction is not exposed.
- `TableValuedSqlParameter`: public sealed TVP type implementing
  `Dapper.SqlMapper.ICustomQueryParameter`; public construction is not exposed.

Constructors:

- No public constructors are exposed.
- `TypedSqlParameter` and `TableValuedSqlParameter` are created only through
  `SqlParam` factories.

`SqlParam` string factories:

- `VarChar(string? value, int size) -> TypedSqlParameter`
  with `SqlDbType.VarChar`, `Size = 1..8000`.
- `NVarChar(string? value, int size) -> TypedSqlParameter`
  with `SqlDbType.NVarChar`, `Size = 1..4000`.
- `Char(string? value, int size) -> TypedSqlParameter`
  with `SqlDbType.Char`, `Size = 1..8000`.
- `NChar(string? value, int size) -> TypedSqlParameter`
  with `SqlDbType.NChar`, `Size = 1..4000`.
- `VarCharMax(string? value) -> TypedSqlParameter`
  with `SqlDbType.VarChar`, `Size = -1`.
- `NVarCharMax(string? value) -> TypedSqlParameter`
  with `SqlDbType.NVarChar`, `Size = -1`.

String nullability and limits:

- `string?` input accepts null and materializes as `DBNull.Value`.
- The library validates declared SQL Server parameter size, not runtime string
  length.
- `varchar` and `char` sizes are declared in SQL Server byte-length terms.
- `nvarchar` and `nchar` sizes are declared in SQL Server character-pair terms.
- `max` is not passed by caller; explicit `Max` factory methods set `Size = -1`.

`SqlParam` numeric factories:

- `Bit(bool? value) -> TypedSqlParameter`, `SqlDbType.Bit`.
- `TinyInt(byte? value) -> TypedSqlParameter`, `SqlDbType.TinyInt`.
- `SmallInt(short? value) -> TypedSqlParameter`, `SqlDbType.SmallInt`.
- `Int(int? value) -> TypedSqlParameter`, `SqlDbType.Int`.
- `BigInt(long? value) -> TypedSqlParameter`, `SqlDbType.BigInt`.
- `Real(float? value) -> TypedSqlParameter`, `SqlDbType.Real`.
- `Float(double? value) -> TypedSqlParameter`, `SqlDbType.Float`.
- `Decimal(decimal? value, byte precision, byte scale) -> TypedSqlParameter`,
  `SqlDbType.Decimal`, `Precision = 1..38`, `Scale = 0..Precision`.
- `Money(decimal? value) -> TypedSqlParameter`, `SqlDbType.Money`.
- `SmallMoney(decimal? value) -> TypedSqlParameter`, `SqlDbType.SmallMoney`.

Numeric behavior:

- Nullable CLR inputs accept null and materialize as `DBNull.Value`.
- CLR types match the SQL Server family without widening factory inputs.
- `Decimal` validates declared metadata and does not round or convert the value.
- Range, overflow, and SQL rounding during execution remain provider/SQL Server
  behavior.

`SqlParam` binary and identifier factories:

- `UniqueIdentifier(Guid? value) -> TypedSqlParameter`,
  `SqlDbType.UniqueIdentifier`.
- `Binary(byte[]? value, int size) -> TypedSqlParameter`,
  `SqlDbType.Binary`, `Size = 1..8000`.
- `VarBinary(byte[]? value, int size) -> TypedSqlParameter`,
  `SqlDbType.VarBinary`, `Size = 1..8000`.
- `VarBinaryMax(byte[]? value) -> TypedSqlParameter`,
  `SqlDbType.VarBinary`, `Size = -1`.

Binary behavior:

- `Guid?` and `byte[]?` inputs accept null and materialize as `DBNull.Value`.
- Empty byte arrays remain empty arrays.
- Byte arrays are not cloned; the caller-provided array reference is assigned to
  the provider parameter.
- The library validates declared binary size, not runtime array length.

`SqlParam` temporal factories:

- `Date(DateOnly? value) -> TypedSqlParameter`, `SqlDbType.Date`.
- `Time(TimeOnly? value, byte scale = 7) -> TypedSqlParameter`,
  `SqlDbType.Time`, `Scale = 0..7`.
- `DateTime(DateTime? value) -> TypedSqlParameter`, `SqlDbType.DateTime`.
- `SmallDateTime(DateTime? value) -> TypedSqlParameter`,
  `SqlDbType.SmallDateTime`.
- `DateTime2(DateTime? value, byte scale = 7) -> TypedSqlParameter`,
  `SqlDbType.DateTime2`, `Scale = 0..7`.
- `DateTimeOffset(DateTimeOffset? value, byte scale = 7) -> TypedSqlParameter`,
  `SqlDbType.DateTimeOffset`, `Scale = 0..7`.

Temporal behavior:

- Nullable temporal inputs accept null and materialize as `DBNull.Value`.
- `DateOnly` is materialized as `DateTime` at midnight.
- `TimeOnly` is materialized as `TimeSpan`.
- `DateTime.Kind` is preserved; the library does not normalize to UTC or convert
  time zones.
- SQL Server scale rounding remains provider/SQL Server behavior.

`SqlParam` TVP factory:

- `TableValued(string typeName, DataTable value) -> TableValuedSqlParameter`.
- `typeName` is non-null, non-empty, and non-whitespace.
- `DataTable` is non-null; empty tables are supported.
- The supplied `DataTable` reference is used directly.

`TypedSqlParameter` public properties and methods:

- `object? Value`: original value before `DBNull.Value` materialization.
- `SqlDbType SqlDbType`: SQL Server provider type.
- `int? Size`: declared size; `-1` means SQL Server `max`; null means no
  declared size.
- `byte? Precision`: declared precision or null.
- `byte? Scale`: declared scale or null.
- `ParameterDirection Direction`: `Input`, `Output`, or `InputOutput`.
- `object? OutputValue`: latest materialized output value, with
  `DBNull.Value -> null`.
- `TypedSqlParameter AsOutput()`: returns a new equivalent output parameter.
- `TypedSqlParameter AsInputOutput()`: returns a new equivalent input/output
  parameter.
- `T? GetValue<T>()`: reads output value using CLR assignability rules only.
- `void AddParameter(IDbCommand command, string name)`: Dapper entry point.

`TableValuedSqlParameter` public properties and methods:

- `string TypeName`: SQL Server user-defined table type name.
- `DataTable Value`: caller-supplied table.
- `SqlDbType SqlDbType`: always `Structured`.
- `ParameterDirection Direction`: always `Input`.
- `void AddParameter(IDbCommand command, string name)`: Dapper entry point.

Materially observable exceptions:

- `ArgumentOutOfRangeException`: invalid string/binary size, decimal precision,
  decimal scale, or temporal scale.
- `ArgumentNullException`: null command, null TVP type name, null TVP
  `DataTable`.
- `ArgumentException`: null, empty, or whitespace parameter name; empty or
  whitespace TVP type name.
- `NotSupportedException`: `AddParameter` receives an `IDbCommand` that is not
  `Microsoft.Data.SqlClient.SqlCommand`.
- `InvalidOperationException`: output read before materialization, or database
  null read as a non-nullable value type.
- `InvalidCastException`: output value is not assignable to `T` in
  `GetValue<T>()`.

## API decisions

- `SqlParam` remains the only public factory entrypoint.
- `TypedSqlParameter` remains public because Dapper callers naturally retain the
  value returned by scalar factories and inspect metadata in tests/diagnostics.
- `TableValuedSqlParameter` remains public because the public factory returns a
  dedicated TVP contract with useful observable metadata and no scalar/output
  APIs.
- No provider-neutral abstraction is introduced.
- No public constructors are introduced.
- No new convenience factories are introduced during freeze.

## KEEP

- Keep all current public factory names and return types.
- Keep nullable scalar inputs and `DBNull.Value` materialization.
- Keep explicit `Max` factory methods instead of caller-provided `-1`.
- Keep decimal precision/scale validation and absence of silent rounding.
- Keep temporal scale default `7` for `Time`, `DateTime2`, and
  `DateTimeOffset`.
- Keep `DateTime.Kind` untouched and no timezone conversion.
- Keep byte array and `DataTable` references un-cloned; callers own mutation
  timing.
- Keep TVPs input-only, `DataTable`-backed, and schema-validation-free.
- Keep `AddParameter` name behavior: use the name supplied by Dapper/caller and
  do not add `@` automatically.
- Keep output lifecycle: read after command execution through the same retained
  instance.
- Keep non-concurrent reuse of the same output instance documented and tested.

## CHANGE BEFORE 1.0

- Changed scalar `TypedSqlParameter.AddParameter` reuse behavior so `Size`,
  `Precision`, and `Scale` are always fully materialized. Declared metadata is
  applied; undeclared scalar metadata is reset to provider default `0`.
- Added unit tests covering stale metadata reset for reused provider parameters.
- Expanded XML documentation for output lifecycle, typed output reads,
  provider-bound `AddParameter`, and non-concurrent output reuse.
- Corrected `CHANGELOG.md`: the published `0.1.0-preview.1` package already
  contains the current public parameter families validated by Prompt 017.

This behavioral change is intentional before `1.0.0` and avoids callers
observing stale scalar metadata from a previously reused `SqlParameter`.

## DEFER

- Additional string/binary overloads.
- Provider-neutral abstractions or a shared core package.
- `System.Data.SqlClient` support.
- `IEnumerable<SqlDataRecord>` TVP support.
- POCO-to-TVP mapping.
- Fluent convenience APIs beyond `AsOutput()` and `AsInputOutput()`.
- Automatic `@` prefix normalization.
- Runtime validation of value length against declared size.
- SQL Server schema introspection or TVP schema validation.
- Array/DataTable defensive copying.

## Nullability

- C# annotations, runtime behavior, XML docs, and tests agree for scalar
  nullable inputs:
  `string?`, `byte[]?`, `Guid?`, `DateOnly?`, `TimeOnly?`, `DateTime?`,
  `DateTimeOffset?`, `decimal?`, and other nullable numeric values.
- Null scalar input remains stored as `Value = null` on `TypedSqlParameter` and
  materializes as `DBNull.Value` on `SqlParameter`.
- `DataTable` for TVPs is not nullable and throws `ArgumentNullException`.
- `OutputValue` is `object?`; output `DBNull.Value` becomes null.
- `GetValue<T>()` returns null only when `T` can represent null; non-nullable
  value types throw for database null.

## Exceptions

- Size, precision, and scale validation failures use
  `ArgumentOutOfRangeException` with useful parameter names.
- Null TVP arguments use `ArgumentNullException`.
- Invalid names use `ArgumentException` with parameter name `name` or
  `typeName`.
- Incompatible provider commands use `NotSupportedException` and include both
  expected and received command types.
- Output lifecycle failures use `InvalidOperationException` with guidance to
  pass the same instance to Dapper and wait for execution completion.
- Incompatible `GetValue<T>()` reads use `InvalidCastException` and include
  requested and actual CLR types.

## Output parameter lifecycle

- `AsOutput()` and `AsInputOutput()` return new scalar instances with equivalent
  value and metadata.
- `OutputValue` and `GetValue<T>()` read from the latest provider parameter
  materialized by `AddParameter`.
- Reading before materialization is rejected.
- Reading should occur only after Dapper command execution completes.
- Non-concurrent reuse is supported; later materialization replaces the retained
  provider parameter.
- Concurrent reuse of one instance across commands remains unsupported.

## TVP contract

- `TableValuedSqlParameter` remains public but not directly constructible.
- TVPs require an explicit non-empty `TypeName`.
- TVPs require a non-null `DataTable`; empty tables are valid.
- TVPs are always `SqlDbType.Structured` and `ParameterDirection.Input`.
- TVPs do not expose scalar metadata or output APIs.
- The caller owns the `DataTable` schema, row contents, and mutation timing.
- SQL Server/provider errors surface during execution for schema mismatches.

## Provider boundary

- The code uses `Microsoft.Data.SqlClient.SqlCommand` and `SqlParameter` only.
- No `System.Data.SqlClient` usage was found.
- `AddParameter` rejects incompatible `IDbCommand` implementations with
  `NotSupportedException`.
- No provider-neutral abstraction or accidental provider-independent contract is
  exposed.

## API parity

- `PublicAPI.Shipped.txt` contains the full candidate 1.0 public API.
- `PublicAPI.Unshipped.txt` contains only `#nullable enable`.
- No public API signature change was made in this prompt.
- Release build and package validation passed for both `net8.0` and `net10.0`.
- No accidental public API difference between TFMs was detected.

## Documentation consistency

- README and examples remain consistent with the supported families and provider
  boundary.
- Output examples already document read timing, `DBNull.Value -> null`, casting,
  and non-concurrent reuse.
- TVP examples document `DataTable`, explicit `TypeName`, empty tables, and
  caller schema responsibility.
- XML documentation was expanded for the output lifecycle and provider-bound
  materialization.
- `CHANGELOG.md` now distinguishes current `Unreleased` stabilization work from
  the actual published contents of `0.1.0-preview.1`.

## Changes performed

- `TypedSqlParameter.AddParameter` now resets undeclared scalar metadata when
  reusing an existing `SqlParameter`.
- Added unit tests for metadata reset on reused scalar parameters.
- Expanded XML docs on `TypedSqlParameter`.
- Updated ADR `0001` with the provider-parameter reuse contract.
- Added Prompt 018 spec and this report.
- Updated stable 1.0 SDD handoff files.
- Corrected `CHANGELOG.md` historical release contents.

## Tests

- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed with 0 warnings and 0 errors.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build`:
  passed, 242 tests.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build`:
  passed, 242 tests.
- `docker version`: passed; client `29.6.2-rd`, server `29.5.3`.
- `docker info`: passed.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build`:
  passed, 35 tests.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build`:
  passed, 35 tests.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`:
  passed.
- `./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`:
  passed.
- `./scripts/Test-PackageConsumption.ps1 -PackageDirectory ./artifacts/packages`:
  passed for `net8.0` and `net10.0` consumers.
- `dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true`:
  passed.
- `dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive`:
  passed; no vulnerable packages reported.
- `git diff --check`: passed.

## Blockers

None.

## Final recommendation

1.0 API FREEZE APPROVED
