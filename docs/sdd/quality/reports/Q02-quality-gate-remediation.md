# SonarQube Quality Gate Remediation

## Failed run

- GitHub Actions run: `31494417537`
- Job: `93788756494`
- Job name: `SonarQube Cloud`
- Branch: `main`
- Commit: `344e09e9d7b4b91119158b99857459162ae746bf`
- Failed step: `End Sonar analysis`
- Scanner message: `QUALITY GATE STATUS: FAILED`

## Scanner result

The scanner reached post-processing, uploaded the analysis, waited for the
Quality Gate, and failed because the gate returned a non-OK result.

## Components confirmed working

- `SONAR_TOKEN`: available in GitHub Actions.
- SonarScanner for .NET: `11.2.1`.
- Build: passed.
- Unit tests: 242 passed.
- Integration tests: 35 passed.
- OpenCover unit report: generated.
- OpenCover integration report: generated.
- Coverage import: successful.
- Analysis upload: successful.
- Quality Gate wait: working.

## Quality Gate conditions

The project is associated with the built-in `Sonar way` Quality Gate.

| Condition | Actual | Required | Result |
| --- | ---: | ---: | --- |
| New Code Coverage | 97.0% derived from first-analysis coverage | >= 80% | PASS |
| New Reliability Rating | 1.0 / A derived from first-analysis rating | A | PASS |
| New Security Rating | 3.0 / C derived from first-analysis rating | A | FAIL |
| New Maintainability Rating | 1.0 / A derived from first-analysis rating | A | PASS |
| Security Hotspots Reviewed on New Code | 100.0% | 100% | PASS |
| Duplicated Lines on New Code | 18.2% derived from first-analysis duplication | <= 3% | FAIL |

The public `api/qualitygates/project_status` response returned `status: NONE`
and no condition values without local `SONAR_TOKEN`. The table above uses public
Sonar measures plus the observed first-analysis New Code behavior. The failed
scanner result is the authenticated source that the gate was non-OK.

## Failed conditions

- New Security Rating: failed because `githubactions:S7637` reported one
  vulnerability in New Code.
- New Duplicated Lines Density: failed because duplicated script lines were
  18.2% while the gate requires at most 3%.

## New Code definition

- New Code definition: `previous_version`
- Source: public SonarCloud settings API, `sonar.leak.period`
- New Code baseline: no previous version baseline was available in the first
  public analysis; the analysis recorded project version `not provided`.
- Evidence: `api/issues/search` with `inNewCodePeriod=true` returned all 13
  unresolved issues.

The current configuration exposed historical content during initial Sonar
adoption. Q02 resolves the concrete repository-side issues instead of changing
the external baseline.

## Coverage

- Overall coverage: `97.0%`
- Line coverage: `97.7%`
- Condition/branch coverage: `94.6%`
- OpenCover reports imported by the failed run: yes.
- Main files with coverage in failed run: 3 of 3.

No evidence indicates a coverage import problem or a New Code Coverage failure.

## Sonar issues

Public Sonar issue search for `main` reported 13 unresolved issues:

| Rule | Type | File | Line | Decision |
| --- | --- | --- | ---: | --- |
| `githubactions:S7637` | Vulnerability | `.github/workflows/release.yml` | 262 | FIX |
| `powershelldre:S8677` | Code smell | `scripts/Test-PackageConsumption.ps1` | 43 | FIX |
| `powershelldre:S8677` | Code smell | `scripts/Test-PublicPackageConsumption.ps1` | 45 | FIX |
| `csharpsquid:S2325` | Code smell | `src/Dapper.TypedParameters.SqlServer/TableValuedSqlParameter.cs` | 41 | ACCEPT WITH JUSTIFICATION |
| `csharpsquid:S2325` | Code smell | `src/Dapper.TypedParameters.SqlServer/TableValuedSqlParameter.cs` | 46 | ACCEPT WITH JUSTIFICATION |

The remaining INFO/code-smell findings do not change the selected remediation
because the maintainability rating is already A and the Quality Gate policy is
unchanged.

## S2325 analysis

### S2325 - TableValuedSqlParameter.SqlDbType

Decision: valid but intentionally accepted with targeted suppression.

Reason: the member is public instance metadata in `PublicAPI.Shipped.txt`, is
documented by the stable 1.0 API review, and matches scalar parameter metadata
ergonomics. Making it static would alter the frozen candidate public contract
for negligible design benefit.

### S2325 - TableValuedSqlParameter.Direction

Decision: valid but intentionally accepted with targeted suppression.

Reason: the member intentionally represents TVP direction as observable
metadata of the parameter instance and is part of the frozen candidate public
contract. Making it static would be a breaking public API change after
`1.0.0-rc.1`.

## Public API compatibility analysis

- `PublicAPI.Shipped.txt`: unchanged.
- `PublicAPI.Unshipped.txt`: unchanged.
- Public API change: none.
- RC compatibility: preserved.

## Selected remediation

- Pin the third-party `NuGet/login` release action to the full commit SHA behind
  `v1`.
- Extract duplicated package-consumption helper code into a shared script.
- Rename the command logging helper to satisfy PowerShell naming conventions.
- Suppress the two S2325 findings locally with explicit compatibility
  justifications.

## Changes performed

- `.github/workflows/release.yml`: `NuGet/login` pinned to
  `8d196754b4036150537f80ac539e15c2f1028841`.
- `scripts/PackageConsumption.Common.ps1`: added shared `Assert-True` and
  `Show-LoggedCommand`.
- `scripts/Test-PackageConsumption.ps1`: dot-sources common helpers.
- `scripts/Test-PublicPackageConsumption.ps1`: dot-sources common helpers.
- `src/Dapper.TypedParameters.SqlServer/TableValuedSqlParameter.cs`: added two
  targeted `SuppressMessage` attributes for S2325.

## Suppressions and justification

Only the two frozen public TVP metadata properties suppress S2325. No global
rule severity, `NoWarn`, `.editorconfig`, Sonar exclusion, coverage exclusion,
or Quality Gate condition was changed.

## Tests

- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed, 0 warnings.
- Unit `net8.0`: passed, 242 tests.
- Unit `net10.0`: passed, 242 tests.
- Integration `net8.0`: passed, 35 tests.
- Integration `net10.0`: passed, 35 tests.

## Public API validation

- Public API files changed: no.
- Public API change: none.

## Package validation

- `dotnet pack`: passed.
- `./scripts/Test-PackageContents.ps1 -PackageDirectory artifacts/packages`:
  passed.
- `./scripts/Test-PackageConsumption.ps1 -PackageDirectory artifacts/packages`:
  passed for `net8.0` and `net10.0`.
- `dotnet msbuild ... -target:RunPackageValidation`: passed.

## OpenCover validation

- Unit OpenCover report exists: yes.
- Integration OpenCover report exists: yes.
- Production assembly: present.
- Production source: present.
- Format: OpenCover.

## External actions required

No SonarCloud configuration change is required for Q02. Branch protection still
requires the human verification already recorded by Q01.

## Deferred maintenance

- `actions/setup-java@v4` deprecation is deferred CI maintenance. It did not
  cause the Quality Gate failure.

## Blockers

None for repository-side remediation.

## Final recommendation

READY TO PUSH AND RE-RUN SONAR
