# TypedParameters.Dapper.PostgreSql

Explicitly typed PostgreSQL parameters for Dapper using `Npgsql`.

## Installation

```bash
dotnet add package TypedParameters.Dapper.PostgreSql --prerelease
```

## Why use it?

Dapper parameter inference is convenient, but PostgreSQL provider metadata can be part of the contract your code already knows. This package makes `NpgsqlDbType` explicit at the call site while preserving ordinary Dapper usage.

## Quick start

```csharp
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;

await using var connection = new NpgsqlConnection(connectionString);

var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT id, external_id, created_at
    FROM customers
    WHERE external_id = @ExternalId
      AND created_at >= @CreatedAt;
    """,
    new
    {
        ExternalId = PostgresParam.Uuid(externalId),
        CreatedAt = PostgresParam.TimestampTz(createdAtUtc)
    });
```

## Supported parameter types

| Family | Factories and metadata |
| --- | --- |
| Strings | `Text`, `VarChar`, `Char` |
| Boolean / numeric | `Boolean`, `SmallInt`, `Integer`, `BigInt`, `Real`, `Double`, `Numeric`, `Money` |
| Binary / UUID | `Bytea`, `Uuid` |
| JSON | `Json`, `Jsonb` |
| Temporal | `Date`, `Time`, `Timestamp`, `TimestampTz`, `Interval` |
| Arrays | `Array<T>(IList<T>? value, NpgsqlDbType elementType)` |

## PostgreSQL-specific semantics

### varchar / char

`PostgresParam.VarChar(value)` sends `character varying`, and `PostgresParam.Char(value)` sends `character`. The current API intentionally does not expose a size argument because `NpgsqlParameter.Size` is not documented here as a PostgreSQL `varchar(n)` or `char(n)` server-side typmod contract.

### numeric

`PostgresParam.Numeric(value)` sends unconstrained PostgreSQL `numeric`. Precision and scale are not public API in this version because the package does not claim a server-side `numeric(p, s)` typmod contract.

### JSON / JSONB

`PostgresParam.Json(value)` maps to PostgreSQL `json`. `PostgresParam.Jsonb(value)` maps to PostgreSQL `jsonb`. The current contract accepts caller-provided JSON text; automatic POCO JSON serialization is outside the package scope.

### timestamp / timestamptz

`PostgresParam.Timestamp(value)` maps to `timestamp without time zone` and accepts `DateTimeKind.Local` or `DateTimeKind.Unspecified` wall-clock values. `PostgresParam.TimestampTz(value)` maps to `timestamp with time zone`, requires `DateTimeKind.Utc`, and does not convert local or unspecified values.

### arrays

`PostgresParam.Array<T>(value, elementType)` requires an explicit scalar `NpgsqlDbType` element type and sends `NpgsqlDbType.Array | elementType`. PostgreSQL arrays are native PostgreSQL parameters, not SQL Server-style table-valued parameters.

## Compatibility

- `net8.0`
- `net10.0`
- Dapper
- Npgsql

## Not currently supported

- PostgreSQL enums
- Composites
- Ranges and multiranges
- PostGIS
- NodaTime
- Automatic POCO JSON serialization
- COPY or bulk APIs
- SQL Server-style output parameters

## Documentation

See the [repository README](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters) and full documentation in the repository `docs/` directory.

## License

MIT.
