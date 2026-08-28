# Dapper.TypedParameters

English | [Português (Brasil)](README.pt-BR.md)

[![NuGet](https://img.shields.io/nuget/v/TypedParameters.Dapper.SqlServer?logo=nuget&label=NuGet)](https://www.nuget.org/packages/TypedParameters.Dapper.SqlServer)
[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=rodri-oliveira-dev_Dapper.TypedParameters&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rodri-oliveira-dev_Dapper.TypedParameters)
[![CI](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters)

`Dapper.TypedParameters` hosts provider-specific packages for explicit database
parameter metadata in Dapper.

```text
Dapper.TypedParameters
├── TypedParameters.Dapper.SqlServer
└── TypedParameters.Dapper.PostgreSql
```

The packages are independent. Use the SQL Server package with
`Microsoft.Data.SqlClient` and `SqlDbType`; use the PostgreSQL package with
`Npgsql` and `NpgsqlDbType`.

The repository intentionally does not expose a shared `TypedDbParameter` base
type. SQL Server and PostgreSQL have different provider APIs and database
semantics, so each package keeps its own small public contract.

## Installation

Install the SQL Server provider:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

Install the PostgreSQL provider:

```bash
dotnet add package TypedParameters.Dapper.PostgreSql
```

Package identities are separate from assembly and namespace identities:

| Package | Assembly | Namespace |
| --- | --- | --- |
| `TypedParameters.Dapper.SqlServer` | `Dapper.TypedParameters.SqlServer.dll` | `Dapper.TypedParameters.SqlServer` |
| `TypedParameters.Dapper.PostgreSql` | `Dapper.TypedParameters.PostgreSql.dll` | `Dapper.TypedParameters.PostgreSql` |

## SQL Server Example

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

## PostgreSQL Example

```csharp
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;

await using var connection = new NpgsqlConnection(connectionString);

var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT id, document, payload, created_at
    FROM customers
    WHERE document = @Document
      AND created_at >= @CreatedAt;
    """,
    new
    {
        Document = PostgresParam.VarChar(document),
        CreatedAt = PostgresParam.TimestampTz(fromUtc)
    });
```

## Why?

Dapper parameter inference is correct and convenient for many scenarios. The
trade-off is that provider metadata is not always obvious in calling code.

When code already knows the database contract, explicit metadata can make that
contract visible and testable:

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

```csharp
new
{
    Payload = PostgresParam.Jsonb(json),
    CreatedAt = PostgresParam.TimestampTz(createdAtUtc)
}
```

The library does not claim that one database type is universally faster than
another. It makes a known contract explicit; performance-sensitive workloads
still need measurement.

## Supported Types

SQL Server:

| Family | SQL Server types |
| --- | --- |
| Strings | `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)`, `nvarchar(max)` |
| Numeric | `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money`, `smallmoney` |
| Binary and identifiers | `uniqueidentifier`, `binary`, `varbinary`, `varbinary(max)` |
| Temporal | `date`, `time`, `datetime`, `smalldatetime`, `datetime2`, `datetimeoffset` |
| Output parameters | `AsOutput()`, `AsInputOutput()`, `OutputValue`, `GetValue<T>()` |
| Table-valued parameters | `SqlDbType.Structured` with explicit `TypeName` and caller-provided `DataTable` |

PostgreSQL:

| Family | Factories | PostgreSQL types |
| --- | --- | --- |
| Text | `Text`, `VarChar`, `Char` | `text`, `character varying`, `character` |
| Boolean/numeric | `Boolean`, `SmallInt`, `Integer`, `BigInt`, `Real`, `Double`, `Numeric`, `Money` | `boolean`, `smallint`, `integer`, `bigint`, `real`, `double precision`, `numeric`, `money` |
| Identifier/binary | `Uuid`, `Bytea` | `uuid`, `bytea` |
| JSON | `Json`, `Jsonb` | `json`, `jsonb` |
| Temporal | `Date`, `Time`, `Timestamp`, `TimestampTz`, `Interval` | `date`, `time without time zone`, `timestamp without time zone`, `timestamp with time zone`, `interval` |
| Arrays | `Array<T>(IList<T>? value, NpgsqlDbType elementType)` | `integer[]`, `uuid[]`, `text[]`, and arrays of other supported v1 scalar element types |

## PostgreSQL Semantics

`PostgresParam.Text(value)` sends PostgreSQL `text`. `VarChar(value)` sends
`character varying`, and `Char(value)` sends `character`. The PostgreSQL API
does not expose `VarChar(value, size)` or `Char(value, size)`: integration tests
show that `NpgsqlParameter.Size` does not make PostgreSQL observe a
`varchar(n)` or `char(n)` typmod, and over-size values are truncated by Npgsql
before reaching the server.

`PostgresParam.Numeric(value)` sends unconstrained PostgreSQL `numeric`.
Precision and scale are not public API in this version because the integration
tests showed `NpgsqlParameter.Precision` and `Scale` as client parameter
metadata, not as a proven server-side `numeric(p, s)` typmod contract.

`PostgresParam.Json(value)` maps to PostgreSQL `json`.
`PostgresParam.Jsonb(value)` maps to PostgreSQL `jsonb`. Version 1 accepts
caller-provided JSON text; automatic POCO serialization is outside the scope of
this package.

Temporal factories follow PostgreSQL/Npgsql semantics:

- `Date(DateOnly?)` sends `date`.
- `Time(TimeOnly?)` sends `time without time zone`.
- `Timestamp(DateTime?)` sends `timestamp without time zone`; it accepts
  `DateTimeKind.Local` or `DateTimeKind.Unspecified` wall-clock values and
  rejects UTC values.
- `TimestampTz(DateTime?)` sends `timestamp with time zone`; it accepts only
  `DateTimeKind.Utc` and does not convert local or unspecified values.
- `Interval(TimeSpan?)` sends `interval`; month and year interval components
  cannot be represented by `TimeSpan`.

`timestamptz` represents an instant. It does not store a time zone identifier.

Arrays are a native PostgreSQL feature. `PostgresParam.Array<T>(value,
elementType)` requires an explicit scalar element `NpgsqlDbType` and sends
`NpgsqlDbType.Array | elementType`. `null` is sent as `DBNull.Value` with the
declared array type; empty arrays remain empty arrays. This is not a SQL Server
TVP equivalent.

## Provider Differences

| Capability | SQL Server provider | PostgreSQL provider |
| --- | --- | --- |
| ADO.NET provider | `Microsoft.Data.SqlClient` | `Npgsql` |
| Type metadata | `SqlDbType` | `NpgsqlDbType` |
| Input parameters | Yes | Yes |
| Output/input-output helpers | Yes | Not in this version |
| Bulk row-shaped parameters | SQL Server TVPs through `DataTable` | No artificial TVP; use PostgreSQL-native patterns outside this package |
| Arrays | Binary arrays as scalar values | Native PostgreSQL arrays with explicit element type |
| JSON | Not modeled by the SQL Server package | `json` and `jsonb` |
| Timestamp semantics | SQL Server temporal types | PostgreSQL `timestamp`/`timestamptz` rules |

## PostgreSQL Out Of Scope

The PostgreSQL package does not support these features in this version:

- PostgreSQL enums;
- composites;
- generic `DataTypeName` UDT APIs;
- ranges;
- multiranges;
- PostGIS;
- network types;
- `hstore`;
- full-text-search types;
- extension-specific types;
- NodaTime;
- automatic POCO JSON serialization;
- `COPY` or bulk APIs;
- schema inspection;
- positional-placeholder rewriting;
- SQL Server-style output parameter parity.

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

## Documentation

- [Getting started with SQL Server](docs/getting-started.md)
- [PostgreSQL provider guide](docs/postgresql.md)
- [Motivation](docs/motivation.md)
- SQL Server examples:
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
- detect implicit conversions;
- automatically choose SQL types;
- map POCOs to SQL Server table-valued parameters;
- create SQL Server user-defined table types;
- serialize POCOs to PostgreSQL JSON;
- emulate SQL Server TVPs in PostgreSQL;
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
dotnet pack src/Dapper.TypedParameters.PostgreSql/Dapper.TypedParameters.PostgreSql.csproj --configuration Release --no-build --output artifacts/packages
```

SQL Server integration tests use Docker and `Testcontainers.MsSql`.
PostgreSQL integration tests use Docker and `Testcontainers.PostgreSql`.

## Release Registries

The protected release workflow accepts a SemVer `version` without a `v` prefix.
It validates, packs, and publishes `TypedParameters.Dapper.SqlServer` and
`TypedParameters.Dapper.PostgreSql` as separate NuGet packages. NuGet.org uses
Trusted Publishing through the `nuget-release` environment; GitHub Packages
uses the ephemeral workflow `GITHUB_TOKEN`.

## Contributing

Issues and pull requests are welcome. Please keep changes small, explicit, and
validated for both supported target frameworks.

## License

This project is licensed under the MIT license.

## Disclaimer

This project is not affiliated with, maintained by, or officially endorsed by
the Dapper project, Microsoft, PostgreSQL, or the Npgsql project.
