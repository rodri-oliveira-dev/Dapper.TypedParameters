# 017 - Public NuGet Consumption Report

## Package ID

```text
TypedParameters.Dapper.SqlServer
```

## Version

```text
0.1.0-preview.1
```

## NuGet Source

```text
https://api.nuget.org/v3/index.json
```

## Indexing/Public Availability Verification

Verified through the NuGet.org V3 flat-container endpoint:

```text
https://api.nuget.org/v3-flatcontainer/typedparameters.dapper.sqlserver/index.json
```

The endpoint returned:

```text
0.1.0-preview.1
```

The validation script also checks this endpoint before creating consumers.

## Cache Isolation Strategy

`scripts/Test-PublicPackageConsumption.ps1` creates a temporary workspace under
`artifacts/`, writes a temporary `NuGet.Config`, clears package sources, adds
only NuGet.org, and sets `NUGET_PACKAGES` to a directory inside the temporary
workspace.

The script does not clear or use the user's global NuGet package cache. It
verifies that the exact package is restored into the isolated cache and that
`.nupkg.metadata` records NuGet.org as the source.

Forbidden package paths were not used:

- `ProjectReference`
- local NuGet source
- `artifacts/packages`
- `HintPath`
- `bin/Release` DLL

## net8.0 Restore Result

```text
net8.0 restore: passed
```

## net8.0 Build Result

```text
net8.0 build: passed
```

## net8.0 Execution Result

```text
net8.0 execution: passed
```

## net10.0 Restore Result

```text
net10.0 restore: passed
```

## net10.0 Build Result

```text
net10.0 build: passed
```

## net10.0 Execution Result

```text
net10.0 execution: passed
```

## APIs Exercised

- `using Dapper.TypedParameters.SqlServer;`
- `SqlParam.VarChar(...)`
- `SqlParam.NVarChar(...)`
- `SqlParam.Int(...)`
- `SqlParam.Decimal(...)`
- `SqlParam.UniqueIdentifier(...)`
- `SqlParam.VarBinary(...)`
- `SqlParam.Date(...)`
- `SqlParam.DateTime2(...)`
- `AsOutput()`
- `AsInputOutput()`
- `SqlParam.TableValued(...)`
- `TypedSqlParameter.AddParameter(...)` with
  `Microsoft.Data.SqlClient.SqlCommand`
- `TableValuedSqlParameter.AddParameter(...)` with
  `Microsoft.Data.SqlClient.SqlCommand`
- `OutputValue`
- `GetValue<T>()`

## Warnings

- The first `git fetch origin` attempt failed because the local `origin` remote
  uses SSH and this session had no SSH key available. The fetch was repeated
  successfully with a temporary HTTPS URL rewrite.
- Local `main` could not be fast-forwarded because it diverged from
  `origin/main`. The working branch `release/1.0-hardening` was created from
  the fetched `origin/main` commit to preserve the remote release baseline
  without merge, rebase, or history rewrite.

## Blockers

None.

## Conclusion

PUBLIC PACKAGE CONSUMPTION PASSED
