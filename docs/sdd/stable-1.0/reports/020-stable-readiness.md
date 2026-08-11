# 1.0.0 Stable Readiness

## Package identity

- Package: `TypedParameters.Dapper.SqlServer`
- Assembly: `Dapper.TypedParameters.SqlServer`
- Namespace: `Dapper.TypedParameters.SqlServer`

No stable package metadata was changed by this prompt.

## Stable version

Stable `1.0.0` was not prepared because the required public RC publication gate
failed.

Current project version remains:

- `VersionPrefix`: `1.0.0`
- `VersionSuffix`: `rc.1`

## RC public consumption

Result:

```text
FAILED
```

Command:

```powershell
./scripts/Test-PublicPackageConsumption.ps1 -PackageVersion 1.0.0-rc.1
```

Observed result:

```text
Package 'TypedParameters.Dapper.SqlServer' version '1.0.0-rc.1' was not found
at 'https://api.nuget.org/v3-flatcontainer/typedparameters.dapper.sqlserver/index.json'.
```

Direct NuGet.org flat-container verification returned only:

```text
0.1.0-preview.1
```

## Public API compatibility with RC

Not evaluated. The published RC package required for the baseline was not
available from NuGet.org.

## Public API freeze

The previously recorded 1.0 public API freeze remains in effect.

No public API changes were made by this prompt.

## Target frameworks

Expected TFMs remain:

- `net8.0`
- `net10.0`

No TFM changes were made.

## Dependencies

Expected dependencies remain centrally managed:

- `Dapper`: current approved repository version.
- `Microsoft.Data.SqlClient`: `6.1.6`.

No dependency upgrades were made.

## SQL Server compatibility

No SQL Server compatibility policy changes were made.

## CI-tested SQL Server

Not revalidated by this prompt because the RC publication gate failed first.

## Unit tests

Not run by this prompt because the RC publication gate failed first.

## Integration tests

Not run by this prompt because the RC publication gate failed first.

## Local package consumption

Not run. Local package consumption cannot replace public RC consumption for the
stable readiness gate.

## Package validation

Not run for stable `1.0.0` because stable package metadata was not prepared.

## RC baseline validation

Not run. `1.0.0-rc.1` was not available as a public NuGet.org baseline.

## Security audit

Not run by this prompt because the RC publication gate failed first.

## Open issue review

GitHub issue review was automated:

```bash
gh issue list --state open --limit 100 --json number,title,labels,state,url
```

Result:

```text
No open issues returned.
```

No release blockers were identified through GitHub Issues.

## Documentation

No README, README.pt-BR, or CHANGELOG stable release changes were made because
the stable preparation gate failed.

## Release workflow

The stable release workflow was not rehearsed by this prompt.

The earlier GitHub workflow query did not show `v1.0.0-rc.1` release workflow
runs, and `gh release view v1.0.0-rc.1` returned:

```text
release not found
```

## Warnings

- The local `main` branch diverged from `origin/main`, so `git pull --ff-only`
  could not complete.
- To avoid rewriting local history, branch `release/1.0.0` was created from the
  updated `origin/main`.

## Blockers

- `RcTagCreated` is not confirmed.
- `RcRehearsal` is not confirmed.
- `RcPublished` is not confirmed.
- `RcPublicConsumption` failed because `1.0.0-rc.1` was not available from
  NuGet.org.

## Final recommendation

BLOCKED
