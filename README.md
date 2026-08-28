# Dapper.TypedParameters

English | [Português (Brasil)](README.pt-BR.md)

[![NuGet](https://img.shields.io/nuget/v/TypedParameters.Dapper.SqlServer?logo=nuget&label=NuGet)](https://www.nuget.org/packages/TypedParameters.Dapper.SqlServer)
[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=rodri-oliveira-dev_Dapper.TypedParameters&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rodri-oliveira-dev_Dapper.TypedParameters)
[![CI](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters)

`Dapper.TypedParameters` hosts provider-specific packages for explicit database
parameter metadata in Dapper.

Available packages:

- `TypedParameters.Dapper.SqlServer`
- `TypedParameters.Dapper.PostgreSql` (preview)

`Dapper.TypedParameters.SqlServer` provides explicit SQL Server parameter
metadata using `Microsoft.Data.SqlClient`.

`Dapper.TypedParameters.PostgreSql` provides explicit PostgreSQL parameter
metadata using `Npgsql`.

Use the SQL Server provider when the database contract is known and the SQL
Server parameter type, size, precision, scale, direction, or table-valued
parameter type name should be visible at the call site.

Use the PostgreSQL provider when the database contract is known and the
PostgreSQL parameter type should be sent as explicit `NpgsqlDbType` metadata.

## Installation

Install the latest stable package from NuGet.org:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

For PostgreSQL:

```bash
dotnet add package TypedParameters.Dapper.PostgreSql --prerelease
```

Official package page:
[TypedParameters.Dapper.SqlServer on NuGet.org](https://www.nuget.org/packages/TypedParameters.Dapper.SqlServer/1.0.0)

For a reproducible 1.0.0 install:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 1.0.0
```

The NuGet package identity is separate from the assembly and namespace:

```text
NuGet package: TypedParameters.Dapper.SqlServer
Assembly: Dapper.TypedParameters.SqlServer.dll
Namespace: Dapper.TypedParameters.SqlServer
```

## Minimal Example

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

```text
.NET string
  -> explicit SQL metadata
  -> SQL Server varchar(11) parameter
```

PostgreSQL preview example:

```csharp
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;

await using var connection = new NpgsqlConnection(connectionString);

var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT id, name
    FROM customers
    WHERE id = @Id;
    """,
    new
    {
        Id = PostgresParam.Uuid(id)
    });
```

## Why?

Dapper parameter inference is correct and convenient for many scenarios. The
trade-off is that the SQL Server metadata sent to the provider is not always
obvious in the calling code.

When code already knows the database contract, explicit metadata can make that
contract visible:

```csharp
new
{
    Document = document
}
```

```csharp
new
{
    Document = SqlParam.VarChar(document, 11)
}
```

The second form does not claim that `varchar` is universally better than
`nvarchar`. It says that this parameter is intended to match a known
`varchar(11)` contract.

## The Problem

SQL Server evaluates parameters using SQL type metadata, not only CLR values. A
metadata mismatch can cause SQL Server conversions depending on the involved
types, type precedence, collation, query shape, indexes, and execution plan.

This library gives the caller control over the parameter metadata sent through
`Microsoft.Data.SqlClient`. It does not guarantee faster queries, remove every
implicit conversion, or analyze execution plans. Measure performance-sensitive
queries in your own workload.

## Supported Parameter Types

| Family | SQL Server types |
| --- | --- |
| Strings | `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)`, `nvarchar(max)` |
| Numeric | `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money`, `smallmoney` |
| Binary and identifiers | `uniqueidentifier`, `binary`, `varbinary`, `varbinary(max)` |
| Temporal | `date`, `time`, `datetime`, `smalldatetime`, `datetime2`, `datetimeoffset` |
| Output parameters | `AsOutput()`, `AsInputOutput()`, `OutputValue`, `GetValue<T>()` |
| Table-valued parameters | `SqlDbType.Structured` with explicit `TypeName` and caller-provided `DataTable` |

Initial PostgreSQL preview support:

| Family | PostgreSQL types |
| --- | --- |
| Strings | `text`, `character varying`, `character` |
| Numeric | `boolean`, `smallint`, `integer`, `bigint`, `real`, `double precision`, `numeric`, `money` |
| Binary and identifiers | `uuid`, `bytea` |
| JSON | `json`, `jsonb` |
| Temporal | `date`, `time without time zone`, `timestamp without time zone`, `timestamp with time zone`, `interval` |

PostgreSQL contracts intentionally follow Npgsql/PostgreSQL semantics rather
than SQL Server parameter symmetry:

- `PostgresParam.VarChar(value)` and `PostgresParam.Char(value)` do not accept a
  size. In Npgsql, `NpgsqlParameter.Size` for these parameters is not observed
  by PostgreSQL as a `varchar(n)` or `char(n)` typmod, and values longer than
  `Size` are truncated before reaching the server.
- `PostgresParam.Numeric(value)` does not accept precision or scale. Npgsql
  materializes `Precision` and `Scale` as client parameter metadata, but
  PostgreSQL still observes an unconstrained `numeric` parameter in the current
  integration tests.
- `PostgresParam.Json(value)` and `PostgresParam.Jsonb(value)` accept JSON text.
  The library does not serialize POCOs or configure Npgsql JSON mapping.
- `PostgresParam.Timestamp(value)` represents `timestamp without time zone` and
  rejects UTC `DateTime` values. Use local or unspecified `DateTime` values for
  wall-clock timestamps.
- `PostgresParam.TimestampTz(value)` represents a UTC instant and accepts only
  `DateTimeKind.Utc`; it does not convert local or unspecified values.
- `PostgresParam.Interval(value)` uses `TimeSpan`. PostgreSQL intervals with
  month or year components cannot be represented by `TimeSpan`.

## Compatibility

| Item | Support |
| --- | --- |
| Target frameworks | `net8.0`; `net10.0` |
| Dapper | `2.1.79` |
| Microsoft.Data.SqlClient | `6.1.6` |
| Npgsql | `10.0.3` |
| ADO.NET providers | `Microsoft.Data.SqlClient` for SQL Server; `Npgsql` for PostgreSQL |
| System.Data.SqlClient | Not supported by the SQL Server provider |
| Declared SQL Server driver compatibility | SQL Server 2016 through SQL Server 2025 |
| CI-tested SQL Server | `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04` |
| CI-tested PostgreSQL | `postgres:17.6-bookworm` |
| Azure SQL Database | Driver-compatible; not integration-tested by this repository |
| Azure SQL Managed Instance | Driver-compatible; not integration-tested by this repository |
| Azure Synapse Analytics | Driver-compatible; not integration-tested by this repository |

The SQL Server and Azure SQL entries describe `Microsoft.Data.SqlClient` driver
compatibility. This repository currently integration-tests only the SQL Server
2022 container image listed above.

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

## Design Principles

- Make provider-specific parameter metadata explicit at the call site.
- Keep the public API small and predictable.
- Use ADO.NET provider types directly.
- Preserve ordinary Dapper calling patterns.
- Prefer explicit factory methods over automatic SQL type selection.
- Avoid cross-provider abstractions until identical responsibilities are proven.

## What This Library Does Not Do

The library does not:

- inspect your database schema;
- query the database for metadata;
- rewrite SQL;
- analyze execution plans;
- detect `CONVERT_IMPLICIT`;
- automatically choose SQL types;
- map POCOs to table-valued parameters;
- create SQL Server user-defined table types;
- support `System.Data.SqlClient`.

## Testing and Quality

The repository validates unit tests, provider integration tests, package
contents, package consumption, public API baselines, SourceLink, package
validation, and SonarQube Cloud Quality Gate checks for the supported target
frameworks.

Basic local validation:

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
```

SQL Server integration tests use Docker and `Testcontainers.MsSql`.
PostgreSQL integration tests use Docker and `Testcontainers.PostgreSql`.

## Release Registries

The protected release workflow publishes the same validated `.nupkg` to
NuGet.org, the primary public installation source, and GitHub Packages, the
repository-linked secondary registry. A rehearsal with `publish=false` never
publishes; `publish=true` requires the matching version tag and approval of the
`nuget-release` environment. NuGet.org uses Trusted Publishing, while GitHub
Packages uses the ephemeral workflow `GITHUB_TOKEN`. After its first
publication, the GitHub package must be made public explicitly before it can be
consumed anonymously.

## Contributing

Issues and pull requests are welcome. Please keep changes small, explicit, and
validated for both supported target frameworks.

## License

This project is licensed under the MIT license.

## Disclaimer

This project is not affiliated with, maintained by, or officially endorsed by
the Dapper project or Microsoft.
