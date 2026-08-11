# Numeric Parameters

English | [Português (Brasil)](numeric.pt-BR.md)

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Numeric factories declare SQL Server numeric and boolean types without relying
on value inference.

| Factory | SQL Server type |
| --- | --- |
| `SqlParam.Bit(value)` | `bit` |
| `SqlParam.TinyInt(value)` | `tinyint` |
| `SqlParam.SmallInt(value)` | `smallint` |
| `SqlParam.Int(value)` | `int` |
| `SqlParam.BigInt(value)` | `bigint` |
| `SqlParam.Real(value)` | `real` |
| `SqlParam.Float(value)` | `float` |
| `SqlParam.Decimal(value, precision, scale)` | `decimal(precision, scale)` |
| `SqlParam.Money(value)` | `money` |
| `SqlParam.SmallMoney(value)` | `smallmoney` |

## Integers and bit

```csharp
await connection.ExecuteAsync(
    """
    UPDATE dbo.Customers
    SET IsActive = @IsActive
    WHERE CustomerId = @CustomerId;
    """,
    new
    {
        CustomerId = SqlParam.Int(42),
        IsActive = SqlParam.Bit(true)
    });
```

## Decimal Precision and Scale

```csharp
Amount = SqlParam.Decimal(123.45M, precision: 18, scale: 2)
```

`precision` and `scale` are explicit parameter metadata. `precision` must be 1
through 38. `scale` must be 0 through `precision`.

The library does not manually round decimal values. Valid conversions, rounding,
and range behavior during execution remain owned by `Microsoft.Data.SqlClient`
and SQL Server.

## Floating-Point and Money Types

```csharp
Temperature = SqlParam.Real(23.5F)
Ratio = SqlParam.Float(0.75D)
Price = SqlParam.Money(12.34M)
Fee = SqlParam.SmallMoney(1.23M)
```

Use `money` and `smallmoney` only when the database contract uses those SQL
Server types. For many application domains, an explicit `decimal(precision,
scale)` contract is easier to reason about.

## Null Values

```csharp
Discount = SqlParam.Decimal(null, precision: 9, scale: 4)
```

`null` is converted to `DBNull.Value` when the parameter is materialized.
