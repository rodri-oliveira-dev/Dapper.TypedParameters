# TypedParameters.Dapper.SqlServer

Explicitly typed SQL Server parameters for Dapper using `Microsoft.Data.SqlClient`.

## Installation

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

## Why use it?

Dapper parameter inference is convenient, but the provider metadata sent to SQL Server is not always obvious at the call site. This package lets code declare the known SQL Server parameter contract explicitly while preserving ordinary Dapper usage.

## Quick start

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

await using var connection = new SqlConnection(connectionString);

var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT Id, Document, Name
    FROM dbo.Customers
    WHERE Document = @Document;
    """,
    new
    {
        Document = SqlParam.VarChar(document, 11)
    });
```

## Supported parameter types

| Family | Factories and metadata |
| --- | --- |
| Strings | `VarChar`, `NVarChar`, `Char`, `NChar`, `VarCharMax`, `NVarCharMax` |
| Numeric | `Bit`, `TinyInt`, `SmallInt`, `Int`, `BigInt`, `Real`, `Float`, `Decimal`, `Money`, `SmallMoney` |
| Binary / identifiers | `UniqueIdentifier`, `Binary`, `VarBinary`, `VarBinaryMax` |
| Temporal | `Date`, `Time`, `DateTime`, `SmallDateTime`, `DateTime2`, `DateTimeOffset` |
| Output parameters | `AsOutput()`, `AsInputOutput()`, `OutputValue`, `GetValue<T>()` |
| Table-valued parameters | `TableValued(typeName, dataTable)` |

## Explicit parameter metadata

`TypedParameters.Dapper.SqlServer` declares SQL Server metadata directly on the `SqlParameter` that Dapper materializes:

- `SqlDbType`
- `Size`
- `Precision`
- `Scale`
- `Direction`

## Output parameters

Scalar parameters can be converted to output or input/output parameters with `AsOutput()` and `AsInputOutput()`. Keep the same typed parameter instance that is passed to Dapper, then read `OutputValue` or `GetValue<T>()` after command execution completes.

## Table-valued parameters

`SqlParam.TableValued(typeName, dataTable)` sends a SQL Server `Structured` parameter. The caller provides the `DataTable` and the name of an existing SQL Server user-defined table type. This package does not create table types, inspect schema, or map POCOs to `DataTable`.

## Compatibility

- `net8.0`
- `net10.0`
- Dapper
- `Microsoft.Data.SqlClient`

## What this package does not do

This package does not inspect database schema, query metadata, rewrite SQL, analyze execution plans, detect implicit conversions, choose SQL types automatically, create user-defined table types, map POCOs to table-valued parameters, or support `System.Data.SqlClient`.

## Documentation

See the [repository README](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters) and full documentation in the repository `docs/` directory.

## License

MIT.
