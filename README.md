# Dapper.TypedParameters

English | [Português (Brasil)](README.pt-BR.md)

`Dapper.TypedParameters.SqlServer` provides explicitly typed SQL Server
parameters for Dapper using `Microsoft.Data.SqlClient`. It helps callers declare
the SQL Server parameter metadata sent to the provider, such as type, size,
precision, scale, direction, and table-valued parameter type name.

## Why?

Dapper makes parameter passing convenient:

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT *
    FROM Customers
    WHERE Document = @Document
    """,
    new
    {
        Document = "12345678901"
    });
```

In this example, `Document` is a .NET `string`. The SQL Server provider needs to
materialize that value as a SQL parameter. Depending on the schema, query, and
provider path, the materialized parameter metadata may not exactly match the
column definition, for example:

```sql
Document varchar(11)
```

This library lets the caller express that intent explicitly when the expected
database type is known.

## The problem

Parameter inference is useful and correct for many Dapper scenarios. The tradeoff
is that the SQL type sent to SQL Server is not always visible at the call site.
When parameter metadata and column metadata differ, SQL Server may need implicit
conversions while evaluating a query.

Those conversions can matter depending on the types involved, data type
precedence, collation, query shape, indexes, and the final execution plan. This
library does not guarantee faster queries. It gives the caller control over the
SQL type metadata sent to SQL Server.

## Before

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT
        Id,
        Document,
        Name
    FROM Customers
    WHERE Document = @Document
    """,
    new
    {
        Document = "12345678901"
    });
```

This code is idiomatic Dapper. It simply does not state that the schema expects a
`varchar(11)` parameter.

## With typed parameters

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT
        Id,
        Document,
        Name
    FROM Customers
    WHERE Document = @Document
    """,
    new
    {
        Document = SqlParam.VarChar("12345678901", 11)
    });
```

```text
.NET string
  -> explicit SQL metadata
  -> varchar(11)
```

`SqlParam.VarChar(...)` is not chosen automatically by the library. The
developer chooses it because they know the database schema.

Unicode columns should be declared explicitly too:

```csharp
Name = SqlParam.NVarChar("João", 150)
```

The library does not assume that `varchar` is better than `nvarchar`. The goal is
explicit correspondence with the schema, not preference for a specific SQL type.

## Installation

Install from NuGet.org after the package is published:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 0.1.0-preview.1
```

To test an unreleased local build from this repository:

```bash
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --output ./artifacts/packages
dotnet add package TypedParameters.Dapper.SqlServer --version 0.1.0-preview.1 --source ./artifacts/packages
```

NuGet package ID:

```text
TypedParameters.Dapper.SqlServer
```

The package has a NuGet identity that is distinct from its assembly and C#
namespace identity:

```text
NuGet package: TypedParameters.Dapper.SqlServer
Assembly: Dapper.TypedParameters.SqlServer.dll
Namespace: Dapper.TypedParameters.SqlServer
```

If NuGet.org does not yet contain the requested version, use the local build
instructions above or wait for the release workflow to publish that version.

## Quick start

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static async Task<Customer?> FindCustomerAsync(
    string connectionString,
    string document)
{
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    return await connection.QuerySingleOrDefaultAsync<Customer>(
        """
        SELECT
            Id,
            Document,
            Name
        FROM dbo.Customers
        WHERE Document = @Document;
        """,
        new
        {
            Document = SqlParam.VarChar(document, 11)
        });
}
```

## Supported parameter types

| Family | SQL Server types |
| --- | --- |
| Strings | `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)`, `nvarchar(max)` |
| Numeric | `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money`, `smallmoney` |
| Binary and identifiers | `uniqueidentifier`, `binary`, `varbinary`, `varbinary(max)` |
| Temporal | `date`, `time`, `datetime`, `smalldatetime`, `datetime2`, `datetimeoffset` |
| Output / InputOutput | Fluent `AsOutput()` and `AsInputOutput()` on scalar parameters |
| Table-valued parameters | `SqlDbType.Structured` with explicit `TypeName` and `DataTable` |

## Compatibility

| Item | Support |
| --- | --- |
| Target frameworks | `net8.0`; `net10.0` |
| Dapper | `2.1.79` |
| Microsoft.Data.SqlClient | `6.1.6` |
| ADO.NET provider | `Microsoft.Data.SqlClient` only |
| System.Data.SqlClient | Not supported |
| Declared driver compatibility target | SQL Server 2016 through SQL Server 2025 |
| CI-tested SQL Server | SQL Server 2022 through `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04` |
| Azure SQL Database | Driver-compatible; not integration-tested by this repository |
| Azure SQL Managed Instance | Driver-compatible; not integration-tested by this repository |
| Azure Synapse Analytics | Driver-compatible; not integration-tested by this repository |

The SQL Server and Azure SQL entries above describe `Microsoft.Data.SqlClient`
driver compatibility. This repository currently integration-tests only the SQL
Server 2022 container image listed in the table.

## Why explicit SQL types can matter

SQL Server evaluates expressions using SQL type metadata, not only CLR values.
Explicit parameter metadata gives the caller control over the SQL type sent to
SQL Server and can help avoid type mismatches when the expected database type is
known.

This is most useful when code and schema are intentionally aligned, such as:

- `varchar(11)` identifiers.
- `nvarchar(150)` names.
- `decimal(18, 2)` amounts.
- `time(0)`, `datetime2(7)`, or `datetimeoffset(7)` values.
- Stored procedure output parameters.
- User-defined table types for TVPs.

Measure performance-sensitive queries in your own workload. This package makes
intent explicit; it does not analyze or optimize execution plans.

## What this library does not do

The library does not:

- inspect your database schema;
- rewrite SQL;
- analyze execution plans;
- automatically detect `CONVERT_IMPLICIT`;
- automatically select the correct SQL type;
- replace Dapper's parameter system;
- change your database schema;
- manage indexes;
- optimize arbitrary queries;
- validate database column definitions.

`SqlParam.VarChar(value, 11)` is an explicit declaration made by the caller. The
library does not know whether the target column is really `varchar(11)`.

## Documentation

- [Getting started](docs/getting-started.md)
- [Motivation](docs/motivation.md)
- Examples:
  - [Strings](docs/examples/strings.md)
  - [Numeric](docs/examples/numeric.md)
  - [Binary and identifiers](docs/examples/binary.md)
  - [Temporal](docs/examples/temporal.md)
  - [Output parameters](docs/examples/output-parameters.md)
  - [Table-valued parameters](docs/examples/table-valued-parameters.md)
- [Português (Brasil)](README.pt-BR.md)

## Testing

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
```

Integration tests use SQL Server through Docker and `Testcontainers.MsSql`.

## Contributing

Issues and pull requests are welcome. Please keep changes small, explicit, and
validated for both supported target frameworks.

## License

This project is licensed under the MIT license.

## Disclaimer

This project is not affiliated with, maintained by, or officially endorsed by
the Dapper project or Microsoft.
