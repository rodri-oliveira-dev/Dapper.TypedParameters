# Dapper.TypedParameters

Typed, provider-specific database parameters for Dapper.

The first package in this repository will be:

```text
Dapper.TypedParameters.SqlServer
```

It will provide an explicit API for creating SQL Server parameters with `Microsoft.Data.SqlClient`, including database type, size, precision, and scale.

## Status

The repository is currently in its bootstrap phase. No production API has been released yet.

## Compatibility

- Dapper 2.1.79
- Microsoft.Data.SqlClient 7.0.2
- .NET 8
- .NET 10

## SQL Server parameters

`SqlParam` creates explicit `Microsoft.Data.SqlClient` parameters for Dapper.

```csharp
await connection.ExecuteAsync(
    "INSERT INTO Events (EventDate, EventTime) VALUES (@EventDate, @EventTime)",
    new
    {
        EventDate = SqlParam.Date(new DateOnly(2026, 8, 5)),
        EventTime = SqlParam.Time(new TimeOnly(12, 30), scale: 0)
    });
```

Available string factories:

- `SqlParam.VarChar(string? value, int size)`
- `SqlParam.NVarChar(string? value, int size)`
- `SqlParam.Char(string? value, int size)`
- `SqlParam.NChar(string? value, int size)`
- `SqlParam.VarCharMax(string? value)`
- `SqlParam.NVarCharMax(string? value)`

Available temporal factories:

- `SqlParam.Date(DateOnly? value)`
- `SqlParam.Time(TimeOnly? value, byte scale = 7)`
- `SqlParam.DateTime(DateTime? value)`
- `SqlParam.SmallDateTime(DateTime? value)`
- `SqlParam.DateTime2(DateTime? value, byte scale = 7)`
- `SqlParam.DateTimeOffset(DateTimeOffset? value, byte scale = 7)`

`time`, `datetime2`, and `datetimeoffset` accept scale values from `0` to `7`.
Temporal parameters do not configure `Size`.

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

Integration tests that require SQL Server run when
`DAPPER_TYPEDPARAMETERS_SQLSERVER_CONNECTION_STRING` is set.

## License

This project is licensed under the MIT License.
