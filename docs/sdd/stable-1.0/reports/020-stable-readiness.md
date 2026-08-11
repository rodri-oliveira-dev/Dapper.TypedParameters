# 1.0.0 Stable Readiness

## Package identity

- Package: `TypedParameters.Dapper.SqlServer`
- Assembly: `Dapper.TypedParameters.SqlServer`
- Namespace: `Dapper.TypedParameters.SqlServer`

Package identity was preserved.

## Stable version

Stable version prepared:

```text
1.0.0
```

The prerelease suffix was removed.

Expected package artifacts:

- `TypedParameters.Dapper.SqlServer.1.0.0.nupkg`
- `TypedParameters.Dapper.SqlServer.1.0.0.snupkg`

Package published by this prompt: No.

## RC public consumption

The published RC was consumed directly from NuGet.org.

Command:

```powershell
./scripts/Test-PublicPackageConsumption.ps1 -PackageVersion 1.0.0-rc.1
```

Source:

```text
https://api.nuget.org/v3/index.json
```

Results:

- `net8.0 restore: passed`
- `net8.0 build: passed`
- `net8.0 execution: passed`
- `net10.0 restore: passed`
- `net10.0 build: passed`
- `net10.0 execution: passed`

A transient file-in-use restore failure occurred on the first `net8.0` attempt;
the script retry succeeded and the final command result was passing.

## Public API compatibility with RC

The stable candidate keeps the same public API contract as `1.0.0-rc.1`.

`PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` were not changed by this
prompt.

## Public API freeze

Public API freeze remains:

```text
Frozen
```

No public API changes were made after the RC.

## Target frameworks

Target frameworks:

- `net8.0`
- `net10.0`

## Dependencies

Dependencies remain centrally managed:

- `Dapper`: `2.1.79`
- `Microsoft.Data.SqlClient`: `6.1.6`

No dependency upgrades were made.

## SQL Server compatibility

The declared compatibility policy remains:

- SQL Server 2016 through SQL Server 2025.
- Azure SQL Database.
- Azure SQL Managed Instance.
- Azure Synapse Analytics.

This policy follows `Microsoft.Data.SqlClient` driver compatibility.

## CI-tested SQL Server

CI integration coverage remains SQL Server 2022 through:

```text
mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04
```

## Unit tests

Passed:

- `net8.0`: 242 tests.
- `net10.0`: 242 tests.

## Integration tests

Passed:

- `net8.0`: 35 tests.
- `net10.0`: 35 tests.

Docker diagnostics passed with:

- Client: `29.6.2-rd`
- Server: `29.5.3`

## Local package consumption

Passed for stable `1.0.0` from `artifacts/packages`.

Results:

- `net8.0 consumer: passed`
- `net10.0 consumer: passed`

A transient file-in-use restore failure occurred on the first `net8.0` attempt;
the script retry succeeded and the final command result was passing.

## Package validation

`PackageValidationBaselineVersion` was updated to:

```text
1.0.0-rc.1
```

SDK package validation passed against the public RC baseline.

Public API validation also passed through the Release build and unchanged public
API analyzer baselines.

## RC baseline validation

Stable `1.0.0` uses public `1.0.0-rc.1` as the SDK package validation baseline.

No package validation suppressions were added.

## Security audit

Vulnerability audit passed.

Command:

```bash
dotnet list Dapper.TypedParameters.sln package --vulnerable --include-transitive
```

Result:

- `Dapper.TypedParameters.SqlServer`: no vulnerable packages.
- `Dapper.TypedParameters.SqlServer.Tests`: no vulnerable packages.
- `Dapper.TypedParameters.SqlServer.IntegrationTests`: no vulnerable packages.

## Open issue review

GitHub issue review was automated:

```bash
gh issue list --state open --limit 100 --json number,title,labels,state,url
```

Result:

```text
No open issues returned.
```

No blockers were identified for:

- bug
- regression
- API
- breaking
- incorrect SQL type
- output parameter
- TVP
- nullability
- package
- restore

## Documentation

Updated:

- `CHANGELOG.md`
- `README.md`
- `README.pt-BR.md`
- stable 1.0 SDD handoff files

Documentation states that this prompt prepares `1.0.0` but does not publish it.

## Release workflow

`.github/workflows/release.yml` was audited.

Confirmed:

- `package_version=1.0.0` is accepted by the version regex.
- `tag=v1.0.0` maps to `refs/tags/v1.0.0`.
- Publish still requires `publish=true`.
- Publish still requires the correct tag ref.
- Publish still uses the `nuget-release` environment.
- Publish still grants `id-token: write` only to the publish job.
- Publish still uses `NuGet/login@v1` Trusted Publishing.
- No long-lived NuGet API key is configured in the workflow.

## Warnings

- The GitHub release for `v1.0.0-rc.1` is not marked as prerelease.
- The successful RC publish run `31490830510` validated, packaged, and published
  `1.0.0-rc.1`.
- Docker diagnostics reported local environment warnings:
  - `No swap limit support`
  - `daemon is not using the default seccomp profile`
- Public and local package consumption each had one transient `net8.0` restore
  file-in-use failure that passed on retry.

## Blockers

None.

## Final recommendation

READY FOR 1.0.0
