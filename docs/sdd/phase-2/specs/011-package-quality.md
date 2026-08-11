# 011 - Package quality and diagnostics

## Context

Prompt 011 improves engineering quality, diagnostics, and NuGet packaging for
`Dapper.TypedParameters.SqlServer` without adding functional API surface.

Prompt 010 is locally present and `docs/sdd/phase-2/STATUS.md` states:

- Last completed prompt: 010
- Current status: Completed
- Next prompt: 011-package-quality

Remote update could not be confirmed in this environment because SSH fetch
failed with `Permission denied (publickey)`. Local `main` was fast-forwarded to
the prompt 010 commit before creating `build/package-quality`.

## Goals

- Produce `.nupkg` and `.snupkg` artifacts in CI without publishing either.
- Embed SourceLink metadata for GitHub-hosted sources.
- Keep deterministic/reproducible build settings explicit.
- Detect accidental public API changes.
- Validate package compatibility and package contents.
- Generate coverage diagnostics in CI without setting arbitrary thresholds.
- Add benchmark coverage for relevant local operations without running full
  benchmarks on pull requests.
- Strengthen dependency and vulnerability diagnostics without adding redundant
  tooling.

## Non-goals

- Add new functional factories, parameter types, or runtime behavior.
- Publish packages, tags, releases, or pull requests.
- Add a compatibility baseline against a package version that has not been
  verified as published.
- Require SQL Server for benchmarks.
- Add coverage gates before measuring current coverage.

## SourceLink

Accepted mechanism: `Microsoft.SourceLink.GitHub`.

Justification:

- Official SourceLink package for GitHub repositories.
- Integrates with SDK pack and symbol packages.
- Does not change runtime behavior or public API.
- Fits the repository URL already declared in MSBuild metadata.

Package metadata/settings:

- `PublishRepositoryUrl=true`
- `EmbedUntrackedSources=true`
- `IncludeSymbols=true`
- `SymbolPackageFormat=snupkg`
- `Deterministic=true`
- `ContinuousIntegrationBuild=true` in CI/GitHub Actions

The CI must upload `.snupkg` alongside `.nupkg`, but must not publish symbols.

## Symbols

Accepted mechanism: SDK symbol package generation with
`SymbolPackageFormat=snupkg`.

Rejected alternatives:

- Legacy `.symbols.nupkg`: rejected because `.snupkg` is the modern NuGet.org
  symbol package format.
- Publishing symbols automatically: rejected because the repository has no
  release/publish automation in this phase.

## Reproducible builds

Accepted mechanism: SDK deterministic build properties plus CI-aware
`ContinuousIntegrationBuild`.

`Deterministic` is already enabled centrally. `ContinuousIntegrationBuild`
remains conditional on `CI` or `GITHUB_ACTIONS` so local builds stay ergonomic
while CI gets stable repository path/source metadata.

## Package validation and API compatibility

Accepted mechanism: SDK package validation and APICompat:

- `EnablePackageValidation=true`
- `EnableStrictModeForCompatibleTfms=true`
- `EnableStrictModeForCompatibleFrameworksInPackage=true`

Justification:

- Official SDK mechanism.
- Validates package assets and API compatibility relationships inside the
  produced package.
- Specifically supports the phase decision that `net8.0` and `net10.0` expose
  equivalent public APIs.
- Avoids a custom reflection/script-based compatibility checker.

Baseline validation:

- No previous public package baseline is configured in this prompt.
- `PackageValidationBaselineVersion` is intentionally omitted because the
  package is still documented as not publicly published.
- A baseline should be added only after a verified published version exists.

Suppressions:

- No compatibility suppression file is introduced.
- Suppressions must be specific and justified if ever required.

## Public API analysis

Accepted mechanism: `Microsoft.CodeAnalysis.PublicApiAnalyzers` with
`PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`.

Justification:

- Widely adopted Roslyn analyzer for explicit public API review.
- Fails builds when public APIs change without updating the baseline.
- Keeps intentional public API changes visible in source review.
- Complements SDK package validation: analyzers catch source changes at build
  time, package validation checks packaged API relationships.

The baseline records the current public API only. No new functional API is
introduced.

## Coverage

Accepted mechanism: existing `coverlet.collector` with `dotnet test
--collect:"XPlat Code Coverage"` in CI.

Justification:

- The repository already centralizes `coverlet.collector` for unit and
  integration tests.
- No new package is needed for raw Cobertura output.
- Reports can be uploaded as artifacts per TFM and per suite.

Threshold decision:

- No threshold is introduced in this prompt.
- Current coverage must be measured first and reported.
- A future threshold may be introduced only with the measured value,
  justification, impact, and an evolution rule.

Integration coverage:

- Unit and integration test coverage are reported separately.
- Integration coverage remains tied to Docker/SQL Server availability in CI.

## Benchmarks

Accepted mechanism: a dedicated
`benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks` project using
BenchmarkDotNet.

Benchmarks cover:

- scalar parameter creation;
- materialization in `SqlCommand`;
- string parameter;
- decimal parameter;
- binary parameter;
- small TVP.

Justification:

- BenchmarkDotNet is the standard .NET benchmarking tool.
- A dedicated project keeps benchmark dependencies out of the library and tests.
- The benchmarked operations are local and do not require SQL Server.

Execution policy:

- Full benchmark runs are not part of PR CI.
- A manual GitHub Actions workflow builds the benchmark project and can run the
  short/full benchmark suite on demand.
- The normal CI only needs build/test/pack/package/coverage validation.

Rejected alternatives:

- Running full benchmarks in every PR: rejected due cost and runtime noise.
- SQL Server benchmark suite in this prompt: rejected because it would require
  separate environment, setup, and maintenance decisions.

## Security and dependencies

Accepted mechanisms:

- Central Package Management remains the only versioning mechanism.
- NuGet audit remains enabled with `NuGetAuditMode=all`.
- GitHub dependency review runs on pull requests.
- `dotnet restore` remains the main vulnerability check path.

Rejected mechanisms:

- `packages.lock.json` / restore locked mode: rejected for this prompt because
  the repository has not made a lock-file maintenance decision and is still in
  preview package shaping.
- Redundant third-party scanners: rejected because NuGetAudit and GitHub
  dependency review cover the immediate package risk without new services.

## Package content validation

CI must inspect produced packages and fail when expected package quality
conditions are not met:

- `.nupkg` exists;
- `.snupkg` exists;
- library assets exist for `net8.0` and `net10.0`;
- XML docs exist for `net8.0` and `net10.0`;
- dependencies include Dapper and Microsoft.Data.SqlClient metadata;
- README is packaged;
- license expression is present in `.nuspec`;
- repository URL is present in `.nuspec`;
- SourceLink metadata exists in the library assemblies;
- no test DLLs are packaged;
- no `bin/` or `obj/` content is packaged;
- no obvious secrets are packaged;
- no temporary files are packaged.

This is implemented as deterministic repository script rather than a large
external package because the checks are package-specific and easy to audit.

## TFM compatibility

`net8.0` and `net10.0` must continue to build and test separately.

The public API must remain equivalent across TFMs through:

- unconditioned source files and package references;
- PublicApiAnalyzers baseline;
- SDK package validation strict compatible TFM checks.

## CI

CI continues to run:

- restore;
- build for `net8.0`;
- build for `net10.0`;
- unit tests for `net8.0`;
- unit tests for `net10.0`;
- integration tests for `net8.0`;
- integration tests for `net10.0`;
- pack;
- package validation;
- package inspection;
- upload `.nupkg`;
- upload `.snupkg`;
- coverage upload.

Benchmarks use a separate manual workflow.

## Maintenance cost

Low/accepted:

- SourceLink and SDK package validation are MSBuild properties.
- Public API baseline updates are explicit and reviewable.
- Coverage uses an existing package.
- Package inspection script is small and deterministic.

Moderate/accepted:

- BenchmarkDotNet introduces a benchmark-only dependency and project, isolated
  from library consumers.

Deferred:

- Lock file governance.
- Published-package baseline compatibility.
- Coverage thresholds.
- SQL Server performance benchmarks.
