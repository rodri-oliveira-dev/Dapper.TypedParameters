# Dapper.TypedParameters

Typed, provider-specific database parameters for Dapper.

The first package in this repository will be:

```text
Dapper.TypedParameters.SqlServer
```

It will provide an explicit API for creating SQL Server parameters with `Microsoft.Data.SqlClient`, including database type, size, precision, and scale.

## Status

The repository is currently in its bootstrap phase. No production API has been released yet.

## Initial compatibility

- Dapper 2.1.79
- Microsoft.Data.SqlClient 7.0.2
- .NET Standard 2.0
- .NET 8

## Repository structure

```text
src/
  Dapper.TypedParameters.SqlServer/
tests/
  Dapper.TypedParameters.SqlServer.Tests/
```

## Build

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## License

This project is licensed under the MIT License.
