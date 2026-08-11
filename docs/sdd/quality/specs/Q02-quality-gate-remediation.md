# Q02 - Quality Gate Remediation

## Status

In progress.

## Context

The first SonarQube Cloud Quality Gate run after Q01 failed on `main`.
Scanner startup, authentication, build, tests, OpenCover generation, coverage
import, analysis upload, and Quality Gate wait were already confirmed working by
the failed run.

## Failed GitHub Actions run

- Run: `31494417537`
- Job: `93788756494`
- Job name: `SonarQube Cloud`
- Branch: `main`
- Commit: `344e09e9d7b4b91119158b99857459162ae746bf`
- Failure step: `End Sonar analysis`

## Known working components

- `SONAR_TOKEN` was available in GitHub Actions.
- SonarScanner for .NET `11.2.1` restored and ran.
- The Sonar project key was `rodri-oliveira-dev_Dapper.TypedParameters`.
- The Sonar organization was `rodri-oliveira-dev`.
- Build succeeded for the canonical Sonar TFM, `net8.0`.
- Unit tests passed and produced OpenCover.
- Integration tests passed and produced OpenCover.
- OpenCover import succeeded.
- Analysis upload succeeded.
- Quality Gate wait executed and returned a non-OK result.

## Unknown root cause

The exact authenticated Quality Gate condition payload is not available locally
because `SONAR_TOKEN` is not present in the local environment. Public APIs expose
project measures, issues, analyses, quality gate configuration, and branch
metadata, but the public `project_status` response currently returns `NONE`
without condition values.

The diagnostic must therefore distinguish real API-observed values from values
that require authenticated or human verification.

## Diagnostic strategy

1. Confirm Q01 completed and Sonar integration is configured.
2. Inspect the failed GitHub Actions job logs through `gh`.
3. Query SonarQube Cloud Web API v1 public endpoints for:
   - project Quality Gate assignment;
   - Quality Gate conditions;
   - branch analysis metadata;
   - project measures;
   - unresolved issues;
   - issues in the New Code period;
   - Security Hotspots.
4. Use `SONAR_TOKEN` only if already available in the environment.
5. Do not change SonarQube Cloud settings remotely.
6. Fix only concrete repository-side issues that preserve the intended policy.

## Quality Gate conditions

The project uses the built-in `Sonar way` gate with Clean as You Code
conditions:

- New Security Rating must be `A`.
- New Reliability Rating must be `A`.
- New Maintainability Rating must be `A`.
- New Code Coverage must be at least `80%`.
- New Duplicated Lines Density must be at most `3%`.
- New Security Hotspots Reviewed must be `100%`.

## New Code definition

The current public setting for `sonar.leak.period` is `previous_version`, inherited
from the SonarCloud instance.

The failed analysis has project version `not provided`. Because this is the
first recorded analysis for the project and issue search with
`inNewCodePeriod=true` returns all unresolved issues, Sonar is effectively
treating historical analyzed code as New Code.

## Coverage analysis

OpenCover import was successful. Public measures for `main` after the failed
analysis are:

- Overall coverage: `97.0%`
- Line coverage: `97.7%`
- Condition/branch coverage: `94.6%`

Public APIs do not expose concrete `new_coverage` values for this analysis
without authentication. No evidence indicates coverage import failure.

## Sonar issues

Public issue search reports:

- `13` unresolved issues.
- `13` unresolved issues in the New Code period.
- `1` vulnerability.
- `12` code smells.
- `0` bugs.
- `0` Security Hotspots.

The actionable repository-side findings are:

- `githubactions:S7637` in `.github/workflows/release.yml`: pin `NuGet/login`
  to a full commit SHA.
- `powershelldre:S8677` in package consumption scripts: command logging helper
  naming or output convention.
- `csharpsquid:S2325` in `TableValuedSqlParameter`: public instance metadata
  properties that are intentionally part of the candidate 1.0 contract.
- duplicated lines concentrated in package consumption scripts.

## Public API compatibility constraints

`TableValuedSqlParameter.SqlDbType` and `TableValuedSqlParameter.Direction` are
recorded in `PublicAPI.Shipped.txt` and in the stable 1.0 API review. They are
part of the frozen `1.0.0-rc.1` candidate public API.

Changing either member from an instance property to a static property would be a
public breaking change and would require restarting the RC cycle.

## Possible remediation strategies

- Fix the real GitHub Actions dependency pinning vulnerability.
- Remove real duplication from validation scripts through a shared helper.
- Fix the PowerShell command logging convention without changing behavior.
- Suppress S2325 locally with an explicit public API compatibility
  justification.
- Document any New Code baseline action that requires human SonarCloud
  configuration.

## Non-goals

- Do not weaken the Quality Gate.
- Do not reduce the New Code Coverage threshold.
- Do not disable Quality Gate wait.
- Do not add `continue-on-error`.
- Do not exclude production code from Sonar or coverage.
- Do not change public API signatures.
- Do not mark Security Hotspots remotely.
- Do not change SonarCloud New Code settings remotely.
- Do not update unrelated deprecated actions in Q02.

## Acceptance criteria

- The failed run and job are documented.
- The actual public Sonar measures and issues are documented.
- Repository-side Sonar issues are remediated without weakening policy.
- `SqlDbType` and `Direction` remain public instance properties.
- Public API compatibility is preserved.
- OpenCover remains configured.
- Quality Gate wait remains enabled.
- No secret value is written to the repository.

## Validation plan

- `dotnet restore Dapper.TypedParameters.sln`
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`
- Unit tests for `net8.0`
- Unit tests for `net10.0`
- Integration tests for `net8.0`
- Integration tests for `net10.0`
- `dotnet pack`
- package contents validation
- package consumption validation
- package validation/API compatibility
- OpenCover generation with `tests/sonar.runsettings`
- `git diff --check`
- diff review for secrets and gate weakening

## Expected files

- `docs/sdd/quality/specs/Q02-quality-gate-remediation.md`
- `docs/sdd/quality/reports/Q02-quality-gate-remediation.md`
- `docs/sdd/quality/STATUS.md`
- `docs/sdd/quality/DECISIONS.md`
- `docs/sdd/quality/EXTERNAL-SETUP.md`
- `.github/workflows/release.yml`
- `scripts/Test-PackageConsumption.ps1`
- `scripts/Test-PublicPackageConsumption.ps1`
- `scripts/PackageConsumption.Common.ps1`
- `src/Dapper.TypedParameters.SqlServer/TableValuedSqlParameter.cs`

## Expected commit

```text
ci: resolve SonarQube quality gate failure
```
