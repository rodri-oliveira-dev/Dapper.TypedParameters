# SonarQube Cloud Integration Report

## Project

- GitHub repository: `rodri-oliveira-dev/Dapper.TypedParameters`.
- SonarQube Cloud project key: `rodri-oliveira-dev_Dapper.TypedParameters`.
- SonarQube Cloud organization key: `rodri-oliveira-dev`.
- Project visibility: public.

## Scanner

- Scanner: SonarScanner for .NET.
- Version: `11.2.1`.
- Installation: local .NET tool manifest in `.config/dotnet-tools.json`.
- Runtime: GitHub Actions configures Java 21 for the Sonar job.

## Authentication

Authentication uses only the GitHub repository Actions secret:

```yaml
${{ secrets.SONAR_TOKEN }}
```

The workflow passes the value to the scanner through the `SONAR_TOKEN`
environment variable and `/d:sonar.token="${SONAR_TOKEN}"`.

## Secret handling

- Secret value stored in repository: no.
- Secret printed in validation output: no.
- Fallback without authentication: no.
- Missing secret behavior: the `SonarQube Cloud` job fails before checkout with
  `SONAR_TOKEN repository secret is required.`

## CI architecture

The existing validation matrix is preserved:

- `Validate net8.0`.
- `Validate net10.0`.
- Unit tests.
- Integration tests.
- Cobertura coverage artifacts.
- Package validation.
- Package consumption.
- NuGet package artifacts.

The new `SonarQube Cloud` job is separate, unique per PR/main analysis path, and
depends on `validate`. The `pack` job still depends only on `validate`.

## Coverage producer

Coverlet through the existing `coverlet.collector` package.

## Coverage format

OpenCover for SonarQube Cloud.

Existing Cobertura artifacts remain in the validation matrix.

## Coverage paths

Sonar imports deterministic OpenCover files through
`sonar.cs.opencover.reportsPaths`:

- `artifacts/coverage/sonar/opencover/unit.opencover.xml`
- `artifacts/coverage/sonar/opencover/integration.opencover.xml`

Local generated report sizes:

- `unit.opencover.xml`: 84679 bytes.
- `integration.opencover.xml`: 84584 bytes.

## Canonical TFM for Sonar

`net8.0`.

Justification: production source has no TFM-specific conditional compilation,
and the existing CI matrix continues validating both `net8.0` and `net10.0`.
`net8.0` is the minimum supported TFM.

## Quality Gate

Detected through SonarCloud public API:

- Quality Gate: `Sonar way`.
- Default gate: yes.

Conditions observed:

- New security rating must not be worse than A.
- New reliability rating must not be worse than A.
- New maintainability rating must not be worse than A.
- New code coverage must be at least 80%.
- New duplicated lines density must be at most 3%.
- New security hotspots reviewed must be 100%.

## New code coverage threshold

80% on new code.

Enforcement authority: SonarQube Cloud Quality Gate.

## Pull request analysis

The workflow runs on `pull_request`. The scanner relies on GitHub Actions pull
request metadata and does not hardcode `sonar.pullrequest.*`.

The Sonar checkout uses `fetch-depth: 0`.

## Main branch analysis

The `SonarQube Cloud` job runs on pushes to `refs/heads/main` and on manual
workflow dispatch.

## Fork PR security

The workflow does not use `pull_request_target`.

Fork pull requests do not receive repository secrets. The job fails early when
`SONAR_TOKEN` is unavailable, preserving secret security instead of running
untrusted PR code in a privileged context.

## Required GitHub check

Required status check to configure after merge:

```text
SonarQube Cloud
```

The SonarQube Cloud decoration check should be reviewed after the first PR
analysis and may also need to be required.

## Local validation

Passed:

- `dotnet tool restore`
- `dotnet restore Dapper.TypedParameters.sln`
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`
- Unit tests `net8.0`: 242 passed.
- Docker diagnostics.
- Integration tests `net8.0`: 35 passed.
- Unit and integration OpenCover generation.
- OpenCover XML structural inspection.
- `dotnet test Dapper.TypedParameters.sln --configuration Release --no-build`
- `dotnet pack`
- Package contents validation.
- Package consumption validation for `net8.0` and `net10.0`.
- YAML parse of `.github/workflows/ci.yml`.
- `git diff --check`.
- Secret-pattern diff review.

## CI validation

CI was not executed remotely in this prompt. No push was performed.

## Warnings

- `git fetch origin` failed locally because the configured SSH remote did not
  have an available public key in this environment.
- Authenticated SonarCloud analysis was not run locally because the secret value
  must not be requested, exposed, or reused outside GitHub Actions.
- Fork PRs cannot run authenticated analysis safely with repository secrets.

## External setup required

After merge, the repository owner must:

1. Push the branch and open a pull request.
2. Observe the first SonarQube Cloud analysis.
3. Confirm that the project uses `Sonar way` or an equivalent Quality Gate with
   New Code Coverage >= 80%.
4. Configure `main` branch protection or a ruleset to require the exact status
   check `SonarQube Cloud`.
5. Confirm whether the SonarQube Cloud decoration check should also be required.
6. Confirm that a red Quality Gate blocks merge.
7. Update `docs/sdd/quality/EXTERNAL-SETUP.md` in later work only after human
   verification.

## Blockers

None for repository-side integration.

External branch protection and first remote Sonar run remain human verification
items.

## Final recommendation

SONARQUBE CLOUD INTEGRATION READY
