# 0007 - Multi-package release policy

## Status

Accepted.

## Context

The repository now contains two provider packages:

- `TypedParameters.Dapper.SqlServer`
- `TypedParameters.Dapper.PostgreSql`

The previous release workflow was designed around one SQL Server package and a
`package_version` plus `publish` input. That shape no longer proves that both
provider packages were validated, packed, consumed, and published from the same
validated commit.

## Decision

- The release workflow accepts a single `version` input without a `v` prefix.
- The workflow must be dispatched from `main`.
- The workflow validates SemVer before restore, build, test, pack, or publish.
- The derived package version is exactly the requested `version`.
- The derived release tag is `v<version>`.
- Both provider project identities are validated through MSBuild before pack:
  `PackageId`, `Version`, and `PackageVersion`.
- The release tag is created only after restore, build, tests, package
  compatibility validation, package content validation, and package consumption
  validation pass.
- An existing release tag is accepted only when it points to the validated
  commit; a tag pointing elsewhere fails the workflow.
- NuGet.org and GitHub Packages publication happen in separate jobs.
- GitHub Release creation happens only after both registries publish
  successfully.
- Prerelease GitHub Releases are inferred from a SemVer prerelease suffix in
  the release tag.

## Consequences

Retrying an interrupted release is safe when the tag already exists on the
validated commit and package pushes encounter existing versions. The workflow no
longer has a rehearsal mode; local validation and ordinary CI are responsible
for rehearsals before a maintainer dispatches the protected release from
`main`.
