# Release decisions

## Accepted

1. PackageId: `TypedParameters.Dapper.SqlServer`.
2. AssemblyName: `Dapper.TypedParameters.SqlServer`.
3. RootNamespace: `Dapper.TypedParameters.SqlServer`.
4. Namespace publico: `Dapper.TypedParameters.SqlServer`.
5. NuGetOwner: `rodri-oliveira-dev`.
6. NuGetOwnership: `Individual`.
7. `Microsoft.Data.SqlClient`: `6.1.6`.
8. `Dapper`: `2.1.79`.
9. TargetFrameworks:
   - `net8.0`
   - `net10.0`
10. PreviewVersion: `0.1.0-preview.1`.
11. ReleaseAuthentication: NuGet Trusted Publishing / GitHub OIDC.
12. LongLivedNuGetApiKey: Not used.
13. Declared driver compatibility target: SQL Server 2016 through SQL Server
    2025 for both TFMs.
14. Azure SQL Database, Azure SQL Managed Instance, and Azure Synapse Analytics
    are driver-compatible through `Microsoft.Data.SqlClient` 6.1.
15. Integration testing in this repository currently uses only SQL Server 2022
    via `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04`.
16. Package publication is prohibited during prompts 13 through 16.
17. PackageIdAvailability: externally verified on NuGet.org public APIs; no
    existing package found for `TypedParameters.Dapper.SqlServer` during prompt
    013.
18. PackageConsumptionValidation: CI must validate that the generated
    `TypedParameters.Dapper.SqlServer` `.nupkg` is consumed by external
    `net8.0` and `net10.0` applications through a local NuGet source, package
    source mapping, an isolated `NUGET_PACKAGES` cache, exact version restore,
    and hash comparison against the local package.
19. Release providers:
    - NuGet.org as the primary public distribution registry.
    - GitHub Packages as a secondary registry linked to this repository.
20. NuGet.org authentication: Trusted Publishing.
21. NuGet.org identity protocol: GitHub Actions OIDC.
22. NuGet owner: `rodri-oliveira-dev`.
23. Ownership: Individual.
24. GitHub environment: `nuget-release`.
25. Workflow: `.github/workflows/release.yml`.
26. Long-lived API key: Forbidden for normal release flow.
27. Prompt 016 external setup evidence: `GitHubEnvironment`,
    `NuGetTrustedPublishingPolicy`, and `HumanVerification` recorded as
    `COMPLETED` by the repository owner in `EXTERNAL-SETUP.md`.
28. Prompt 016 Package ID status: available for first publication on NuGet.org
    public APIs at audit time.
29. Prompt 016 release readiness: `READY FOR PREVIEW RELEASE`.
30. Package published during Prompt 016: No.
31. Push, pull request, and tag during Prompt 016: No.
32. GitHub Packages owner: `rodri-oliveira-dev`.
33. GitHub Packages feed:
    `https://nuget.pkg.github.com/rodri-oliveira-dev/index.json`.
34. GitHub Packages authentication: the ephemeral workflow `GITHUB_TOKEN`;
    no long-lived package token is permitted.
35. GitHub Packages permission: `packages: write`, restricted to the GitHub
    Packages publication job.
36. Repository association: package metadata `RepositoryUrl` must remain
    `https://github.com/rodri-oliveira-dev/Dapper.TypedParameters`.
37. Protected publication contract: the release workflow is dispatched from
    `main` with a SemVer `version` input without `v`; it validates, builds,
    tests, packs, and consumes the packages before creating or verifying the
    matching `v<version>` tag.
38. Retry behavior: both pushes use `--skip-duplicate` so an interrupted
    protected release can be resumed without replacing an existing version.
39. GitHub Packages visibility: after the first publication, the repository
    owner must explicitly make the package public before treating the feed as
    anonymously consumable. NuGet.org remains the documented installation
    source.
40. GitHub Packages validation: pre-publication package content, compatibility,
    and consumption checks apply to the exact artifact sent to both registries;
    the push command exit status gates registry acceptance. Anonymous
    post-publication consumption is not a release gate while package visibility
    can still be private.
41. Multi-package release contract: the release workflow accepts one SemVer
    `version` input without a `v` prefix and derives `v<version>` as the release
    tag.
42. The release workflow must be dispatched from `main`; the tag is created or
    verified only after validation, build, tests, package validation, content
    validation, and package consumption validation pass for both
    `TypedParameters.Dapper.SqlServer` and `TypedParameters.Dapper.PostgreSql`.
43. NuGet.org and GitHub Packages publication run in separate jobs, with
    explicit publish steps per package and `--skip-duplicate` for retry safety.
44. GitHub Release creation runs only after both registry publication jobs
    complete and uploads both `.nupkg` and `.snupkg` artifacts.
