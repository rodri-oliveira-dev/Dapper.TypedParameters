# String Parameters

[English](strings.md) | [Português (Brasil)](strings.pt-BR.md)

[Back to README](../../README.md) | [Getting started](../getting-started.md)

String factories declare SQL Server string metadata explicitly.

| Factory | SQL Server type | Size |
| --- | --- | --- |
| `SqlParam.VarChar(value, size)` | `varchar(size)` | 1 to 8,000 |
| `SqlParam.NVarChar(value, size)` | `nvarchar(size)` | 1 to 4,000 |
| `SqlParam.Char(value, size)` | `char(size)` | 1 to 8,000 |
| `SqlParam.NChar(value, size)` | `nchar(size)` | 1 to 4,000 |
| `SqlParam.VarCharMax(value)` | `varchar(max)` | `Size = -1` |
| `SqlParam.NVarCharMax(value)` | `nvarchar(max)` | `Size = -1` |

## varchar and nvarchar

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT Id, Document, Name
    FROM dbo.Customers
    WHERE Document = @Document
      AND Name = @Name;
    """,
    new
    {
        Document = SqlParam.VarChar("12345678901", 11),
        Name = SqlParam.NVarChar("João", 150)
    });
```

The library does not assume that `varchar` is better than `nvarchar`. Choose the
factory that matches the schema or stored procedure contract.

## Fixed-length strings

```csharp
StateCode = SqlParam.Char("SP", 2)
LanguageCode = SqlParam.NChar("A", 1)
```

The library declares the fixed SQL type and size. SQL Server and
`Microsoft.Data.SqlClient` own fixed-length storage behavior.

## max types

```csharp
AnsiPayload = SqlParam.VarCharMax(payload)
UnicodePayload = SqlParam.NVarCharMax(payload)
```

Use the explicit `max` factories instead of passing `-1` to bounded factories.

## Null values

```csharp
Nickname = SqlParam.NVarChar(null, 80)
```

`null` is materialized as `DBNull.Value`.

## Size notes

For `varchar` and `char`, SQL Server sizes are declared in bytes. The
relationship between bytes and characters depends on the data and SQL Server
configuration. The library validates the declared size range but does not
validate whether a particular value fits in bytes.
