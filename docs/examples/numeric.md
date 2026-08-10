# Numeric Parameters

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

## Decimal precision and scale

```csharp
await connection.ExecuteAsync(
    """
    INSERT INTO dbo.Invoices (CustomerId, Amount)
    VALUES (@CustomerId, @Amount);
    """,
    new
    {
        CustomerId = SqlParam.BigInt(123456789L),
        Amount = SqlParam.Decimal(123.45M, precision: 18, scale: 2)
    });
```

`precision` must be 1 through 38. `scale` must be 0 through `precision`.

The library does not manually round decimal values. Valid conversions, rounding,
and range behavior during execution remain owned by `Microsoft.Data.SqlClient`
and SQL Server.

## Floating-point and money

```csharp
Temperature = SqlParam.Real(23.5F)
Ratio = SqlParam.Float(0.75D)
Price = SqlParam.Money(12.34M)
Fee = SqlParam.SmallMoney(1.23M)
```

Use these factories only when the database contract uses the corresponding SQL
Server type.

## Null values

```csharp
Discount = SqlParam.Decimal(null, precision: 9, scale: 4)
```

`null` is materialized as `DBNull.Value`.
