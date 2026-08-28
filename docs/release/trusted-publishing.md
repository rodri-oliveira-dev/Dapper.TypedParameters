# NuGet Trusted Publishing setup

This repository publishes `TypedParameters.Dapper.SqlServer` and
`TypedParameters.Dapper.PostgreSql` through NuGet.org Trusted Publishing and
GitHub Actions OIDC. The normal release flow must not use a long-lived
`NUGET_API_KEY` secret.

## GitHub

Create a GitHub Environment:

```text
nuget-release
```

Recommended protection:

- Add at least one required reviewer.
- Enable deployment protection appropriate for NuGet release approval.
- Restrict deployment refs to the protected `main` branch. The workflow creates
  or verifies the release tag after validation, but the protected NuGet.org job
  still runs from `refs/heads/main`.

- Do not add a permanent NuGet API key secret.
- Do not create a `NUGET_API_KEY` repository or environment secret for the
  normal release flow.

The release workflow references this environment only in the NuGet.org
publishing job:

```yaml
environment: nuget-release
```

Validation, tag creation, GitHub Packages publication, and GitHub Release
creation do not request NuGet credentials.

## NuGet.org

Create one Trusted Publishing policy for each NuGet package using GitHub
Actions.

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

The workflow must be dispatched from:

```text
refs/heads/main
```

The `version` input must be SemVer without a `v` prefix. The workflow derives
the release tag as:

```text
v<version>
```

For example:

```text
version: 1.1.0
release tag: v1.1.0
```

The workflow validates, builds, tests, packs, checks package contents, and
checks package consumption before creating the tag. If the tag already exists
on the same commit, retry continues idempotently. If the tag points to another
commit, the workflow fails.

## Before Prompt 16

After this prompt is completed and committed, the repository owner must
configure the GitHub environment and NuGet Trusted Publishing policy, then
manually update `docs/sdd/release/EXTERNAL-SETUP.md` before Prompt 16.

Do not commit that manual status update before Prompt 16.
