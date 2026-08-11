# Q01 - SonarQube Cloud Quality Gate

## Status

Completed.

## Context

The repository already has a public SonarQube Cloud project:
`rodri-oliveira-dev_Dapper.TypedParameters`.

The project is public and belongs to organization `rodri-oliveira-dev`, as
confirmed through the public SonarCloud API.

## Problem

Pull requests and the main branch need SonarQube Cloud analysis with imported
.NET coverage. A pull request must not be considered valid when the SonarQube
Cloud Quality Gate is not passed.

## Goals

- Add one SonarQube Cloud analysis per commit or pull request.
- Use SonarScanner for .NET with a pinned local tool version.
- Import .NET coverage in OpenCover format.
- Preserve the existing `net8.0` and `net10.0` CI validation matrix.
- Fail the workflow when the analysis fails, coverage reports are missing, the
  Quality Gate fails, or the Quality Gate times out.
- Keep `SONAR_TOKEN` only as a GitHub Actions repository secret reference.

## Non-goals

- Do not publish a package, tag, release, or pull request.
- Do not change production API, package identity, runtime dependencies, or TFMs.
- Do not duplicate the SonarQube Quality Gate with a separate global coverage
  threshold in GitHub Actions.
- Do not configure GitHub branch protection or SonarCloud settings remotely.

## Current CI Architecture

`.github/workflows/ci.yml` currently contains:

- `Dependency review` on pull requests.
- `Validate net8.0` and `Validate net10.0` matrix jobs.
- Unit tests for each TFM.
- Integration tests for each TFM.
- Coverlet collector through `XPlat Code Coverage`.
- Cobertura coverage artifacts for unit and integration tests.
- `Pack NuGet artifact` after validation.
- Package contents validation.
- Package consumption validation.
- NuGet package and symbol package artifacts.

The Sonar integration must be additive.

## SonarQube Cloud Project

- Project key: `rodri-oliveira-dev_Dapper.TypedParameters`.
- Organization key: `rodri-oliveira-dev`.
- Visibility: public.
- Current analyses: none found through public API at the time of Q01.

## Authentication Strategy

The only authentication source is:

```yaml
${{ secrets.SONAR_TOKEN }}
```

The workflow must fail early with a clear message if the secret is unavailable.
No token value may be printed, stored, copied to artifacts, or committed.

## Scanner Strategy

Use SonarScanner for .NET as a local .NET tool through
`.config/dotnet-tools.json`.

Selected version: `11.2.1`.

Official sources used:

- SonarScanner for .NET installing and using pages from SonarQube Cloud docs.
- GitHub Actions page from SonarQube Cloud docs.
- SonarQube Cloud scanner environment general requirements page.
- SonarSource GitHub release for `11.2.1.137242`.

The Sonar job explicitly configures Java 21 to satisfy the current SonarQube
Cloud scanner runtime requirement.

## Coverage Strategy

The existing CI matrix keeps Cobertura artifacts. The Sonar job collects its own
canonical `net8.0` coverage through Coverlet collector and a dedicated
`tests/sonar.runsettings` file that emits OpenCover.

The OpenCover files are normalized to deterministic paths before scanner end:

- `artifacts/coverage/sonar/opencover/unit.opencover.xml`
- `artifacts/coverage/sonar/opencover/integration.opencover.xml`

Sonar imports them through `sonar.cs.opencover.reportsPaths`.

## Quality Gate Strategy

`sonar.qualitygate.wait=true` is enabled in the scanner begin step.

`sonar.qualitygate.timeout=300` is explicit. A failed gate or timeout causes a
non-zero scanner result and fails the job.

The Quality Gate remains the authority for:

- Coverage on New Code >= 80%.
- No new reliability issues.
- No new security issues.
- No new maintainability issues.
- All new Security Hotspots reviewed.
- Duplicated Lines on New Code <= 3%.

## Pull Request Behavior

The workflow runs on `pull_request`. The scanner relies on GitHub Actions
metadata for pull request detection instead of hardcoding
`sonar.pullrequest.*` properties.

The Sonar checkout uses `fetch-depth: 0` so new-code and pull request analysis
have enough history.

## Main Branch Behavior

The workflow runs on `push`, including `main`, so the main branch analysis can
stay current after merge.

## Fork Pull Request Security

The repository is public. `pull_request_target` is not used.

Fork pull requests do not receive repository secrets from GitHub Actions. The
Sonar job fails before checkout or analysis when `SONAR_TOKEN` is unavailable.
This preserves secret security and documents that fork PRs need a trusted
maintainer path before authenticated analysis can run.

## Branch Protection Requirements

After merge, the repository owner must configure `main` branch protection or a
ruleset requiring the stable GitHub status check:

```text
SonarQube Cloud
```

The SonarQube Cloud decoration check may also appear and should be considered
when configuring required checks.

## Failure Scenarios

- Missing `SONAR_TOKEN`: job fails before checkout.
- Missing OpenCover report: job fails before scanner end.
- OpenCover report without production assembly/source: job fails before scanner
  end.
- Sonar analysis failure: job fails.
- Quality Gate failure: job fails.
- Quality Gate timeout: job fails.

## Acceptance Criteria

- Existing CI validations are preserved.
- A single Sonar job is added.
- Scanner is restored from local tool manifest.
- Coverage is OpenCover and produced by Coverlet.
- `sonar.cs.opencover.reportsPaths` points to deterministic files.
- `sonar.qualitygate.wait=true` is configured.
- No `continue-on-error` exists in Sonar gate steps.
- No secret value is committed.
- SDD files document decisions, external setup, status, and report.

## Validation Plan

- `dotnet tool restore`
- `dotnet restore Dapper.TypedParameters.sln`
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`
- Unit tests for `net8.0`
- OpenCover generation for canonical `net8.0`
- XML structural inspection for production assembly/source
- `git diff --check`
- Security scan of diff for accidental secrets

Integration tests for `net8.0` will be attempted when Docker is available.

## Validation Results

Completed locally without running authenticated SonarCloud analysis:

- `dotnet tool restore`: passed.
- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`:
  passed.
- Unit tests `net8.0`: passed, 242 tests.
- Unit OpenCover generation `net8.0`: passed.
- `docker version` and `docker info`: passed.
- Integration tests `net8.0`: passed, 35 tests.
- Integration OpenCover generation `net8.0`: passed.
- OpenCover structural inspection: production assembly and production source
  files found.
- `dotnet test Dapper.TypedParameters.sln --configuration Release --no-build`:
  passed for unit and integration tests on `net8.0` and `net10.0`.
- `dotnet pack`: passed.
- Package contents validation: passed.
- Package consumption validation: passed for `net8.0` and `net10.0`.
- `git diff --check`: passed.
- YAML parse of `.github/workflows/ci.yml`: passed.
- Secret pattern review in diff: passed.

Limitations:

- `git fetch origin` failed locally because the configured SSH remote did not
  have an available public key in this environment.
- Authenticated SonarCloud analysis was not run locally because the token value
  must not be requested or exposed.

## Expected Files

- `.config/dotnet-tools.json`
- `.github/workflows/ci.yml`
- `tests/sonar.runsettings`
- `CHANGELOG.md`
- `docs/sdd/quality/README.md`
- `docs/sdd/quality/DECISIONS.md`
- `docs/sdd/quality/STATUS.md`
- `docs/sdd/quality/EXTERNAL-SETUP.md`
- `docs/sdd/quality/specs/Q01-sonarqube-cloud-quality-gate.md`
- `docs/sdd/quality/reports/Q01-sonarqube-cloud-quality-gate.md`

## Expected Commit

```text
ci: enforce SonarQube quality gate
```
