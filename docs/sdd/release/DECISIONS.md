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
