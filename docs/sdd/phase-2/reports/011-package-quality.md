# 011 - Package quality report

## Tools chosen

- `Microsoft.SourceLink.GitHub` `10.0.301` for GitHub SourceLink metadata.
- SDK symbol package generation with `SymbolPackageFormat=snupkg`.
- SDK package validation/APICompat with strict compatible TFM checks.
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` `5.6.0` with shipped/unshipped
  public API baseline files.
- Existing `coverlet.collector` for Cobertura coverage collection.
- `BenchmarkDotNet` `0.15.8` in an isolated benchmark project.
- `scripts/Test-PackageContents.ps1` for deterministic package content checks.
- GitHub dependency review on pull requests.
- NuGet audit and `dotnet list package --vulnerable --include-transitive`.

## Tools rejected

- Legacy `.symbols.nupkg`: replaced by modern `.snupkg`.
- Automatic package/symbol publishing: out of scope for this phase.
- Published-package baseline validation: deferred because no verified previous
  public package baseline exists.
- Public API suppressions: not needed; no suppression file was added.
- Restore locked mode: deferred until a repository lock-file maintenance
  decision exists.
- Coverage threshold: rejected for this prompt because current coverage had to
  be measured first.
- Full benchmarks on every PR: rejected due cost, runtime variability, and lack
  of need for every code review.
- SQL Server performance benchmarks: deferred to a separate explicit manual
  suite if needed later.
- Additional third-party security scanners: not added because NuGet audit and
  dependency review cover the immediate dependency risk.

## Build results

- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed.
- Build output covered `net8.0` and `net10.0` for library, unit tests, and
  integration tests.
- Build warnings: 0.

## Test results

- `dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build`: passed.
  - Unit tests: 240 passed.
  - Integration tests: 35 passed.
- `dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build`: passed.
  - Unit tests: 240 passed.
  - Integration tests: 35 passed.

## Coverage

- Unit coverage `net8.0`: 97.77% line, 95.16% branch.
- Unit coverage `net10.0`: 97.77% line, 95.16% branch.
- Integration coverage `net8.0`: 65.00% line, 59.67% branch.
- Integration coverage `net10.0`: 65.00% line, 59.67% branch.
- No threshold was configured. The measured values should be used before any
  future gate is proposed.

## Package contents

- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed.
- Produced:
  - `Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`
  - `Dapper.TypedParameters.SqlServer.0.1.0-preview.1.snupkg`
- `.nupkg` entries:
  - `Dapper.TypedParameters.SqlServer.nuspec`
  - `README.md`
  - `lib/net8.0/Dapper.TypedParameters.SqlServer.dll`
  - `lib/net8.0/Dapper.TypedParameters.SqlServer.xml`
  - `lib/net10.0/Dapper.TypedParameters.SqlServer.dll`
  - `lib/net10.0/Dapper.TypedParameters.SqlServer.xml`
  - NuGet metadata files.
- `.snupkg` entries:
  - `Dapper.TypedParameters.SqlServer.nuspec`
  - `lib/net8.0/Dapper.TypedParameters.SqlServer.pdb`
  - `lib/net10.0/Dapper.TypedParameters.SqlServer.pdb`
  - NuGet metadata files.
- `scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`: passed.
- Validated package metadata: README, MIT license expression, repository URL,
  dependencies, symbols, SourceLink metadata, and absence of test DLLs,
  `bin`/`obj`, temporary files, and obvious secret patterns.

## API compatibility

- Public API baseline created at
  `src/Dapper.TypedParameters.SqlServer/PublicAPI.Shipped.txt`.
- `src/Dapper.TypedParameters.SqlServer/PublicAPI.Unshipped.txt` remains empty
  except for `#nullable enable`.
- The shipped baseline records 46 public symbols.
- `dotnet msbuild src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj -target:RunPackageValidation -property:Configuration=Release -property:NoBuild=true`: passed.
- SDK package validation strict compatible TFM checks passed for `net8.0` and
  `net10.0`.
- No APICompat suppressions were added.

## Warnings

- Build warnings: 0.
- Pack warnings: 0 observed.
- Package validation warnings: 0 observed.

## Vulnerabilities

- `dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive`: no vulnerable packages reported.
- `dotnet list benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks.csproj package --vulnerable --include-transitive`: no vulnerable packages reported.
- NuGet audit also ran during restore with `NuGetAuditMode=all`.

## Benchmarks

- Created `benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks`.
- Benchmarks cover parameter creation, materialization in `SqlCommand`, string,
  decimal, binary, and small TVP scenarios.
- `dotnet restore benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks.csproj`: passed.
- `dotnet build benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks.csproj --configuration Release --no-restore`: passed for `net8.0` and `net10.0`.
- Full benchmark measurement was not run, by design.
- Manual workflow added in `.github/workflows/benchmarks.yml`.

## Blockers

- No critical blocker for the local implementation.
- Remote update over SSH failed with `Permission denied (publickey)`, so the
  remote `main` could not be confirmed from this environment. Local `main` was
  fast-forwarded to the prompt 010 branch before creating
  `build/package-quality`.

## Future recommendations

- Add `PackageValidationBaselineVersion` only after a verified public package
  baseline exists.
- Decide whether the repository wants lock files and restore locked mode before
  adding them.
- Consider a coverage threshold only after reviewing current coverage, risk,
  impact on contributions, and an explicit evolution rule.
- Add SQL Server performance benchmarks only as a separate manual suite with
  clear environment requirements.
- Periodically review `PublicAPI.Shipped.txt` during release preparation.
