# NuGet Trusted Publishing setup

This repository publishes `TypedParameters.Dapper.SqlServer` through NuGet.org
Trusted Publishing and GitHub Actions OIDC. The normal release flow must not
use a long-lived `NUGET_API_KEY` secret.

## GitHub

Create a GitHub Environment:

```text
nuget-release
```

Recommended protection:

- Add at least one required reviewer.
- Enable deployment protection appropriate for NuGet release approval.
- Restrict deployment refs to release tags matching:

```text
v*
```

- Do not add a permanent NuGet API key secret.
- Do not create a `NUGET_API_KEY` repository or environment secret for the
  normal release flow.

The release workflow references this environment only in the publishing job:

```yaml
environment: nuget-release
```

Rehearsal runs with `publish=false` do not use the environment and do not
request NuGet credentials.

## NuGet.org

Create a Trusted Publishing policy for GitHub Actions.

Use these values exactly:

| Field | Value |
| --- | --- |
| Owner | `rodri-oliveira-dev` |
| Ownership | Individual |
| Repository Owner | `rodri-oliveira-dev` |
| Repository | `Dapper.TypedParameters` |
| Workflow File | `release.yml` |
| Environment | `nuget-release` |

For the workflow field, use only the workflow file name expected by the NuGet
interface:

```text
release.yml
```

Do not enter:

```text
.github/workflows/release.yml
```

NuGet.org will match the GitHub OIDC token from
`.github/workflows/release.yml` to the configured workflow file name and
environment.

## Release ref policy

The workflow requires the release ref to be:

```text
refs/tags/v<package_version>
```

For example:

```text
package_version: 0.1.0-preview.1
required tag: v0.1.0-preview.1
```

If `publish=true` is selected from any other ref, the workflow fails before
requesting the temporary NuGet credential.

## Before Prompt 16

After this prompt is completed and committed, the repository owner must
configure the GitHub environment and NuGet Trusted Publishing policy, then
manually update `docs/sdd/release/EXTERNAL-SETUP.md` before Prompt 16.

Do not commit that manual status update before Prompt 16.
