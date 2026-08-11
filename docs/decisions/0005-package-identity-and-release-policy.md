# 0005 - Package identity and release policy

## Status

Accepted.

## Context

The first public NuGet package needs a final package identity before
publication. Earlier repository decisions used `Dapper.TypedParameters.SqlServer`
as the project, assembly, namespace, and package-oriented identity. The package
name is now separated from the binary and C# namespace identity.

## Decision

- The public NuGet Package ID is `TypedParameters.Dapper.SqlServer`.
- The NuGet owner is `rodri-oliveira-dev`.
- The NuGet ownership type is Individual.
- The assembly name remains `Dapper.TypedParameters.SqlServer`.
- The assembly file remains `Dapper.TypedParameters.SqlServer.dll`.
- The root namespace and public namespace remain
  `Dapper.TypedParameters.SqlServer`.
- The repository name, solution name, and project names are not renamed.
- The first planned preview version remains `0.1.0-preview.1`.
- The package uses `Dapper` `2.1.79`.
- The package uses `Microsoft.Data.SqlClient` `6.1.6`.
- `System.Data.SqlClient` remains unsupported.
- Release authentication should use NuGet Trusted Publishing with GitHub OIDC.
- Long-lived NuGet API keys are not used for release automation.
- No package is published automatically during release-preparation prompts.

## SQL Server policy

The declared driver compatibility target, through `Microsoft.Data.SqlClient`
6.1, is SQL Server 2016 through SQL Server 2025 for both `net8.0` and
`net10.0`.

This repository currently integration-tests only SQL Server 2022 through:

```text
mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04
```

Documentation must distinguish driver compatibility from CI-tested SQL Server
coverage.

## Azure policy

Through the `Microsoft.Data.SqlClient` 6.1 compatibility matrix, the package is
driver-compatible with:

- Azure SQL Database;
- Azure SQL Managed Instance;
- Azure Synapse Analytics.

This repository does not integration-test those Azure services and does not add
Azure infrastructure as part of the first preview policy.

## Target framework policy

The first preview offers assets for both `net8.0` and `net10.0`. The public API
must remain equivalent between those TFMs. Removing a TFM in the future requires
an explicit compatibility decision.

## Consequences

Consumers install the package by NuGet ID
`TypedParameters.Dapper.SqlServer`, but keep using:

```csharp
using Dapper.TypedParameters.SqlServer;
```

Package files are expected to be named from the NuGet Package ID, while the
assemblies inside `lib/net8.0` and `lib/net10.0` keep the existing assembly
name.
