# 005 - Preview package readiness report

## Status

Completed.

## Package

- Version: `0.1.0-preview.1`.
- Package ID: `Dapper.TypedParameters.SqlServer`.
- TFMs: `net8.0`; `net10.0`.
- Title: `Dapper Typed Parameters for SQL Server`.
- Description: `Explicit SQL Server parameter types for Dapper using Microsoft.Data.SqlClient.`
- Authors: `Rodrigo de Oliveira`.
- License: `MIT` through NuGet license expression.
- README in package: `README.md`.
- Repository URL: `https://github.com/rodri-oliveira-dev/Dapper.TypedParameters`.
- Repository type: `git`.
- Tags: `dapper;sql-server;sqlclient;ado.net;parameters;micro-orm`.
- Deterministic build: `true`.
- Symbols: not configured; no `.snupkg` was generated.

## Dependencies

The `.nuspec` contains identical dependency groups for `net8.0` and `net10.0`:

- `Dapper` `2.1.79`, excluding `Build,Analyzers`.
- `Microsoft.Data.SqlClient` `7.0.2`, excluding `Build,Analyzers`.

No unnecessary conditional package references were found in the library project.

## Package Files

The generated package was:

```text
artifacts/packages/Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg
```

ZIP inspection found these entries:

```text
[Content_Types].xml
_rels/.rels
Dapper.TypedParameters.SqlServer.nuspec
lib/net10.0/Dapper.TypedParameters.SqlServer.dll
lib/net10.0/Dapper.TypedParameters.SqlServer.xml
lib/net8.0/Dapper.TypedParameters.SqlServer.dll
lib/net8.0/Dapper.TypedParameters.SqlServer.xml
package/services/metadata/core-properties/bef377088a664424afc43a545cc36b17.psmdcp
README.md
```

Confirmed:

- `.nuspec` file present.
- `README.md` present at package root.
- `lib/net8.0/Dapper.TypedParameters.SqlServer.dll` present.
- `lib/net10.0/Dapper.TypedParameters.SqlServer.dll` present.
- XML documentation present for both TFMs.
- License represented as `<license type="expression">MIT</license>`.
- No test DLLs.
- No `bin/` or `obj/` paths.
- No source files.
- No secret-like file names.
- No duplicate file paths.

## Validation Results

- `dotnet --info`: passed. Active SDK `10.0.302`; installed SDKs include `8.0.423`, `10.0.110`, `10.0.204`, and `10.0.302`.
- `dotnet restore Dapper.TypedParameters.sln`: passed; all projects were up to date for restore.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build`: passed; 29 passed, 0 failed, 0 skipped.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build`: passed; 29 passed, 0 failed, 0 skipped.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build`: passed; 8 passed, 0 failed, 0 skipped.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build`: passed; 8 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed; generated version `0.1.0-preview.1`.
- `git diff --check`: passed.

## Public API Equivalence

The public API was inspected from:

- `src/Dapper.TypedParameters.SqlServer/bin/Release/net8.0/Dapper.TypedParameters.SqlServer.dll`.
- `src/Dapper.TypedParameters.SqlServer/bin/Release/net10.0/Dapper.TypedParameters.SqlServer.dll`.

Both TFMs expose the same public API:

```text
type Dapper.TypedParameters.SqlServer.SqlParam
  method TypedSqlParameter Char(string value, int size)
  method TypedSqlParameter NChar(string value, int size)
  method TypedSqlParameter NVarChar(string value, int size)
  method TypedSqlParameter NVarCharMax(string value)
  method TypedSqlParameter VarChar(string value, int size)
  method TypedSqlParameter VarCharMax(string value)
type Dapper.TypedParameters.SqlServer.TypedSqlParameter : Dapper.SqlMapper.ICustomQueryParameter
  property int? Size
  property SqlDbType SqlDbType
  property object Value
  method void AddParameter(IDbCommand command, string name)
```

No public API differences were found between `net8.0` and `net10.0`.

## Warnings

- Build and test commands reported 0 warnings.
- `git diff --check` reported no whitespace issues.
- Package ID values beginning with `Dapper` may be subject to NuGet reserved prefix rules. The package was not renamed because that requires an explicit project decision.

## Blockers

None.

## Readiness Recommendation

Ready for human pull request review. Do not publish the package until the Package ID and any NuGet reserved prefix implications are reviewed by the project owner.

## Suggested PR Title

```text
feat: add SQL Server typed string parameters
```

## Suggested PR Body

```markdown
## Context

This PR prepares the first preview of `Dapper.TypedParameters.SqlServer`, focused on explicit SQL Server string parameters for Dapper using `Microsoft.Data.SqlClient`.

## Problem

Dapper and the SQL Server provider may infer string parameters in ways that are not explicit at the call site. Consumers need a small API to declare SQL Server string parameter metadata intentionally.

## Solution

Adds typed parameter factories for SQL Server string types and validates them through unit tests, SQL Server integration tests, CI, documentation, and local NuGet package inspection.

## API Added

- `SqlParam.VarChar(value, size)`
- `SqlParam.NVarChar(value, size)`
- `SqlParam.Char(value, size)`
- `SqlParam.NChar(value, size)`
- `SqlParam.VarCharMax(value)`
- `SqlParam.NVarCharMax(value)`
- `TypedSqlParameter`

## Compatibility

- Supports `net8.0` and `net10.0` in one package.
- Public API is equivalent across both TFMs.
- Supports only `Microsoft.Data.SqlClient`.
- Does not support `System.Data.SqlClient`.

## Unit Tests

- `net8.0`: 29 passed, 0 failed, 0 skipped.
- `net10.0`: 29 passed, 0 failed, 0 skipped.

## Integration Tests

- `net8.0`: 8 passed, 0 failed, 0 skipped.
- `net10.0`: 8 passed, 0 failed, 0 skipped.
- Integration uses SQL Server container image `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04` through `Testcontainers.MsSql`.

## CI

- CI validates `net8.0` and `net10.0` explicitly.
- Unit and integration tests run separately per TFM.
- Pack runs only after validation.
- No NuGet publish step is configured.

## Documentation

- README documents motivation, compatibility, install-from-local-package flow, API, examples, limits, null behavior, non-goals, tests, roadmap, affiliation, and license.
- CHANGELOG documents `0.1.0-preview.1`.
- SDD specs and readiness report capture validation evidence.

## Risks

- Integration tests require Docker and enough resources to start SQL Server.
- The package has not been published to NuGet.
- Package IDs beginning with `Dapper` may be subject to NuGet reserved prefix rules.

## Limitations

- This preview covers only string parameters.
- No schema inspection, execution plan analysis, output parameters, TVPs, lists, or provider-neutral abstractions are included.

## Validation Checklist

- [x] Restore passed.
- [x] Build passed with 0 warnings.
- [x] Unit tests passed for `net8.0`.
- [x] Unit tests passed for `net10.0`.
- [x] Integration tests passed for `net8.0`.
- [x] Integration tests passed for `net10.0`.
- [x] Local pack generated `0.1.0-preview.1`.
- [x] Package contains `net8.0` and `net10.0` assets.
- [x] Package contains README.
- [x] Package metadata and dependencies were inspected.
- [x] Public API is equivalent across TFMs.

## Package ID Note

The planned Package ID is `Dapper.TypedParameters.SqlServer`. Because IDs beginning with `Dapper` may be subject to NuGet reserved prefix rules, the owner should review this before publication. This PR does not rename the package.
```

## Release Actions

- Push performed: No.
- Package published: No.
- Pull request opened: No.
- Tag created: No.
- Release created: No.
