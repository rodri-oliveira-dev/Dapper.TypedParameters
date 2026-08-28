# PostgreSQL Provider Guide

English | [Português (Brasil)](postgresql.pt-BR.md)

[Back to README](../README.md) | [SQL Server getting started](getting-started.md)

This guide covers the PostgreSQL package:

```text
NuGet package: TypedParameters.Dapper.PostgreSql
Assembly: Dapper.TypedParameters.PostgreSql.dll
Namespace: Dapper.TypedParameters.PostgreSql
ADO.NET provider: Npgsql
```

The package exists to make PostgreSQL parameter metadata explicit at the Dapper
call site. It sends `NpgsqlDbType` directly and does not inspect schema, rewrite
SQL, or infer database types from CLR values.

## Getting Started

Install the package:

```bash
dotnet add package TypedParameters.Dapper.PostgreSql
```

Use the PostgreSQL namespace with Dapper and Npgsql:

```csharp
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;
```

```csharp
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

## Supported Types

| Family | Factories | PostgreSQL types |
| --- | --- | --- |
| Text | `Text`, `VarChar`, `Char` | `text`, `character varying`, `character` |
| Boolean/numeric | `Boolean`, `SmallInt`, `Integer`, `BigInt`, `Real`, `Double`, `Numeric`, `Money` | `boolean`, `smallint`, `integer`, `bigint`, `real`, `double precision`, `numeric`, `money` |
| Identifier/binary | `Uuid`, `Bytea` | `uuid`, `bytea` |
| JSON | `Json`, `Jsonb` | `json`, `jsonb` |
| Temporal | `Date`, `Time`, `Timestamp`, `TimestampTz`, `Interval` | `date`, `time without time zone`, `timestamp without time zone`, `timestamp with time zone`, `interval` |
| Arrays | `Array<T>(IList<T>? value, NpgsqlDbType elementType)` | arrays using `NpgsqlDbType.Array | elementType` |

All factories return `TypedPostgresParameter`, which exposes:

```csharp
public object? Value { get; }
public NpgsqlDbType NpgsqlDbType { get; }
```

When Dapper materializes the parameter, `null` becomes `DBNull.Value`,
`NpgsqlDbType` is assigned explicitly, and parameter direction is `Input`.

## Text, Varchar, Char, And Size

`PostgresParam.Text(value)` sends `NpgsqlDbType.Text`.
`PostgresParam.VarChar(value)` sends `NpgsqlDbType.Varchar`.
`PostgresParam.Char(value)` sends `NpgsqlDbType.Char`.

The PostgreSQL provider does not expose size-bearing `VarChar` or `Char`
factories. Integration tests verified this behavior with raw `NpgsqlParameter`
metadata:

| Metadata | PostgreSQL observes | Size behavior observed |
| --- | --- | --- |
| `NpgsqlDbType.Varchar` | `character varying` | no `varchar(n)` typmod observed |
| `NpgsqlDbType.Char` | `character` | no `char(n)` typmod observed |
| `NpgsqlParameter.Size = 3` | same backend type | over-size values were truncated before reaching PostgreSQL |

This is intentionally different from the SQL Server provider, where
`SqlParameter.Size` is part of the SQL Server parameter contract exposed by the
package.

## Numeric, Precision, And Scale

`PostgresParam.Numeric(value)` sends `NpgsqlDbType.Numeric`.

Precision and scale are not public PostgreSQL API in this version. Integration
tests using raw `NpgsqlParameter.Precision` and `Scale` showed PostgreSQL
observing the backend type as `numeric`, while a value beyond the declared
metadata round-tripped without provider rounding, truncation, or server-side
`numeric(p, s)` validation.

The package therefore documents `numeric` as an explicit PostgreSQL type, not as
a typmod declaration mechanism.

## JSON

`PostgresParam.Json(value)` maps to PostgreSQL `json`.
`PostgresParam.Jsonb(value)` maps to PostgreSQL `jsonb`.

Version 1 accepts JSON text supplied by the caller:

```csharp
new
{
    Payload = PostgresParam.Jsonb("{\"active\":true}")
}
```

Automatic POCO serialization, `System.Text.Json` policy choices,
`JsonDocument`, and global Npgsql JSON mapping are outside this package's v1
scope.

## Temporal

PostgreSQL temporal factories intentionally follow Npgsql/PostgreSQL semantics:

| Factory | CLR value | PostgreSQL type | Contract |
| --- | --- | --- | --- |
| `Date` | `DateOnly?` | `date` | calendar date |
| `Time` | `TimeOnly?` | `time without time zone` | wall-clock time |
| `Timestamp` | `DateTime?` | `timestamp without time zone` | wall-clock timestamp; accepts `Local` and `Unspecified`, rejects `Utc` |
| `TimestampTz` | `DateTime?` | `timestamp with time zone` | UTC instant; accepts only `DateTimeKind.Utc` |
| `Interval` | `TimeSpan?` | `interval` | duration representable by `TimeSpan` |

`TimestampTz` does not convert local or unspecified values. Callers must pass a
UTC `DateTime`. PostgreSQL `timestamptz` represents an instant and does not
store a time zone identifier.

`Timestamp` represents `timestamp without time zone`. It is for wall-clock
values and rejects UTC `DateTime` values so the call site does not blur an
instant with a local timestamp.

`Interval` uses `TimeSpan`. PostgreSQL intervals can include month and year
components, which `TimeSpan` cannot represent.

## Arrays

PostgreSQL arrays are native provider-specific features. They are not modeled
as SQL Server TVPs.

```csharp
new
{
    CustomerIds = PostgresParam.Array(customerIds, NpgsqlDbType.Integer),
    ExternalIds = PostgresParam.Array(externalIds, NpgsqlDbType.Uuid),
    Tags = PostgresParam.Array(tags, NpgsqlDbType.Text)
}
```

Supported behavior:

- `elementType` is explicit and must be a supported scalar `NpgsqlDbType`.
- `integer[]`, `uuid[]`, and `text[]` are integration-tested.
- `T[]` and `List<T>` are accepted through `IList<T>` without copying.
- `null` is sent as `DBNull.Value` with the declared array type.
- empty arrays remain empty arrays.

Limitations:

- `elementType` must not already include `Array`, `Range`, or `Multirange`.
- arrays of arrays are not supported in this version.
- arrays whose element type requires `DataTypeName` or custom Npgsql mapping
  are outside this version.

## Provider Differences And Limitations

| Area | SQL Server | PostgreSQL |
| --- | --- | --- |
| Provider package | `TypedParameters.Dapper.SqlServer` | `TypedParameters.Dapper.PostgreSql` |
| Provider type metadata | `SqlDbType` | `NpgsqlDbType` |
| ADO.NET provider | `Microsoft.Data.SqlClient` | `Npgsql` |
| Output/input-output API | Supported by scalar SQL Server parameters | Not copied in this version |
| Row-shaped bulk parameter | SQL Server TVP with explicit `TypeName` and `DataTable` | No artificial TVP abstraction |
| Arrays | No generic SQL Server array feature | Native PostgreSQL arrays |
| JSON | Not exposed in the SQL Server package | `json` and `jsonb` |
| Temporal semantics | SQL Server type family | PostgreSQL `timestamp` and `timestamptz` rules |

There is no shared `TypedDbParameter` because a common base would hide important
provider-specific behavior. Duplication between providers is accepted where it
keeps each database contract explicit.

The PostgreSQL package does not support:

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
