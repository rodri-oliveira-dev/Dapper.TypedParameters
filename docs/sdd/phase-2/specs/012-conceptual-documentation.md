# 012 - Conceptual documentation and multilingual README

## Status

Completed

## Context

The first generation of `Dapper.TypedParameters.SqlServer` now has functional
coverage for string, numeric, binary, identifier, temporal, output, input/output,
and table-valued SQL Server parameters. Before release preparation continues,
the public documentation needs to make the library's value proposition clear for
external users who have not followed the SDD history.

## Problem

The library has real functionality, but a new user needs to understand quickly
why it exists. The documentation must not present the project as merely a
`SqlParameter` wrapper. It needs to explain the practical mismatch that can
exist between .NET values, provider-created SQL Server parameters, and the SQL
types declared by a database schema.

For example, a `.NET string` may be used against a `varchar(11)` column. The
important concept is not that inference is wrong, but that inference does not
always express the exact SQL Server metadata the caller intends to send.

## Goals

- Rewrite the main `README.md` in English.
- Add `README.pt-BR.md` as a maintained Brazilian Portuguese translation.
- Explain the conceptual problem behind explicitly typed SQL Server parameters.
- Include clear before/after examples.
- Add deeper documentation under `docs/`.
- Add parameter-family documentation for every family present in the real API.
- Document compatibility: target frameworks, Dapper, `Microsoft.Data.SqlClient`,
  provider scope, and SQL Server support state.
- Document limitations and non-goals.
- Keep links consistent and relative.
- Ensure snippets match the real public API.

## Non-goals

- No code changes.
- No query optimization implementation.
- No automatic execution plan diagnostics.
- No performance guarantees.
- No final release definition.
- No definitive Package ID decision beyond the currently configured project
  metadata.
- No dependency updates.

## Information Architecture

Public documentation will be organized as:

```text
README.md
README.pt-BR.md
docs/
  getting-started.md
  motivation.md
  examples/
    strings.md
    numeric.md
    binary.md
    temporal.md
    output-parameters.md
    table-valued-parameters.md
```

All requested parameter-family documents are applicable because the current
public API exposes string, numeric, binary/identifier, temporal, output,
input/output, and table-valued parameter support.

## Public Documentation Principles

- English is the canonical public documentation language.
- `README.pt-BR.md` is a maintained translation of the canonical README.
- Snippets must reflect the real API in `SqlParam`, `TypedSqlParameter`, and
  `TableValuedSqlParameter`.
- Documentation must not promise behavior that is not tested.
- Examples should favor clarity over concision.
- Performance concepts must be described cautiously and conditionally.
- Explicit SQL type selection is the caller's responsibility because the package
  does not inspect database schema.

## Acceptance Criteria

- `README.md` is in English and links to `README.pt-BR.md` near the top.
- `README.pt-BR.md` exists, links back to `README.md`, and contains equivalent
  functional information.
- `docs/getting-started.md` and `docs/motivation.md` exist in English.
- Example documents exist only for API families implemented in the real code.
- All snippets using `SqlParam`, `TypedSqlParameter`, `AsOutput`,
  `AsInputOutput`, `GetValue`, and `TableValued` match the shipped public API.
- Installation text does not claim the package is already published to NuGet.
- Compatibility text reflects actual TFMs and dependency versions.
- Limitations explicitly say the package does not inspect schema, rewrite SQL,
  analyze plans, detect `CONVERT_IMPLICIT`, choose the correct SQL type
  automatically, replace Dapper, change schema, manage indexes, optimize
  arbitrary queries, or validate column definitions.
- `CHANGELOG.md`, `docs/sdd/phase-2/STATUS.md`, and
  `docs/sdd/phase-2/DECISIONS.md` are updated.
- No functional code, target framework, package ID, or dependency version is
  changed.
- Exactly one commit is created with the planned message.

## Validation Plan

```bash
dotnet --info
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
docker version
docker info
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
pwsh ./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages
git diff --check
```

## Initial Discovery

- Requested branch was `docs/conceptual-documentation`, but the user explicitly
  asked to use the current branch after the initial SSH fetch blocker.
- Working branch: `build/package-quality`.
- `git fetch origin` failed with `Permission denied (publickey)`, matching the
  existing phase-2 handoff note from prompt 011.
- Working tree initially had one unrelated untracked file: `prompts.md`.
- Local phase-2 status shows prompt 011 completed.
- Current package ID: `Dapper.TypedParameters.SqlServer`.
- Current target frameworks: `net8.0`; `net10.0`.
- Current Dapper version: `2.1.79`.
- Current `Microsoft.Data.SqlClient` version: `7.0.2`.
- Current provider scope: `Microsoft.Data.SqlClient` only;
  `System.Data.SqlClient` is not supported.
- Current integration tests include SQL Server container coverage for Dapper,
  output parameters, temporal parameters, and table-valued parameters.

## Planned Commit

```text
docs: explain typed parameter use cases
```

## Files Created

- `README.pt-BR.md`
- `docs/getting-started.md`
- `docs/motivation.md`
- `docs/examples/strings.md`
- `docs/examples/numeric.md`
- `docs/examples/binary.md`
- `docs/examples/temporal.md`
- `docs/examples/output-parameters.md`
- `docs/examples/table-valued-parameters.md`
- `docs/sdd/phase-2/specs/012-conceptual-documentation.md`

## Files Changed

- `README.md`
- `CHANGELOG.md`
- `docs/sdd/phase-2/DECISIONS.md`
- `docs/sdd/phase-2/README.md`
- `docs/sdd/phase-2/STATUS.md`

## API Documented

- Strings: `VarChar`, `NVarChar`, `Char`, `NChar`, `VarCharMax`,
  `NVarCharMax`.
- Numeric: `Bit`, `TinyInt`, `SmallInt`, `Int`, `BigInt`, `Real`, `Float`,
  `Decimal`, `Money`, `SmallMoney`.
- Binary and identifiers: `UniqueIdentifier`, `Binary`, `VarBinary`,
  `VarBinaryMax`.
- Temporal: `Date`, `Time`, `DateTime`, `SmallDateTime`, `DateTime2`,
  `DateTimeOffset`.
- Scalar output direction and reads: `AsOutput`, `AsInputOutput`,
  `OutputValue`, `GetValue<T>()`.
- Table-valued parameters: `TableValued(string typeName, DataTable value)`.

## Validation Results

- `dotnet --info`: passed. Active SDK: `10.0.302`; installed SDKs include
  `8.0.423`, `10.0.110`, `10.0.204`, and `10.0.302`.
- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed; 0 warnings, 0 errors.
- Unit tests `net8.0`: passed; 240 passed, 0 failed, 0 skipped.
- Unit tests `net10.0`: passed; 240 passed, 0 failed, 0 skipped.
- `docker version`: passed; client `29.6.2-rd`, server `29.5.3`.
- `docker info`: passed; Rancher Desktop WSL Distribution, Linux containers,
  8 CPUs, 15.32 GiB memory.
- Integration tests `net8.0`: passed; 35 passed, 0 failed, 0 skipped.
- Integration tests `net10.0`: passed; 35 passed, 0 failed, 0 skipped.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`:
  passed; generated `.nupkg` and `.snupkg`.
- `pwsh ./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages`:
  not executed because `pwsh` is not installed or not in `PATH`.
- `.\scripts\Test-PackageContents.ps1 -PackageDirectory .\artifacts\packages`:
  passed under Windows PowerShell.
- Package inspection confirmed `README.md`, `lib/net8.0` and `lib/net10.0`
  assets, and XML documentation files.
- `git diff --check`: passed.
- Manual/scripted relative link check for public documentation: passed.
- Search for `TODO`, `TBD`, `placeholder`, `example.com`, and unsafe
  performance promises found no required changes.

## Tests Not Executed

None. Unit and integration tests were executed for both real TFMs.

## Limitations

- Remote `origin` could not be fetched because SSH authentication failed.
- The requested branch name was not created; the user explicitly instructed use
  of the current branch.
- `prompts.md` remains an unrelated untracked file and was not staged.
- The package has not been published to NuGet.
- No push or pull request was created.
- The formal SQL Server support matrix remains pending for the next release
  policy prompt.

## Decisions

- English is the canonical public documentation language.
- `README.pt-BR.md` is a maintained Portuguese translation.
- Deep technical documentation lives under `docs/`.
- Documentation must not promise performance gains.
- The package does not inspect schema or choose SQL types automatically.
- The caller remains responsible for matching SQL parameter metadata to the
  database schema or stored procedure contract.

## Final Status

Completed.

## Follow-up: Portuguese Documentation Pages

After the initial prompt 012 commit, the user requested Brazilian Portuguese
versions for the documentation pages linked from the README. The follow-up adds:

- `docs/getting-started.pt-BR.md`
- `docs/motivation.pt-BR.md`
- `docs/examples/strings.pt-BR.md`
- `docs/examples/numeric.pt-BR.md`
- `docs/examples/binary.pt-BR.md`
- `docs/examples/temporal.pt-BR.md`
- `docs/examples/output-parameters.pt-BR.md`
- `docs/examples/table-valued-parameters.pt-BR.md`

The English documents now link to their Portuguese counterparts, and
`README.pt-BR.md` links directly to the Portuguese documentation set.

Follow-up validation:

- Relative Markdown link check across README and `docs/`: passed.
- Snippet search for documented `SqlParam` APIs: reviewed against the existing
  public API; no code changes were required.
- Search for placeholders and unsafe absolute performance promises: no new
  issue found.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed; 0 warnings, 0 errors.
- `git diff --check`: passed.
