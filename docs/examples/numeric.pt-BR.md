# Parâmetros Numéricos

[English](numeric.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Factories numéricas declaram tipos SQL Server numéricos e booleanos sem depender
de inferência pelo valor.

| Factory | Tipo SQL Server |
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

## Inteiros e bit

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

## Precisão e escala de decimal

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

`precision` deve estar entre 1 e 38. `scale` deve estar entre 0 e `precision`.

A biblioteca não arredonda valores decimal manualmente. Conversões válidas,
arredondamento e comportamento de faixa durante a execução continuam sob
responsabilidade de `Microsoft.Data.SqlClient` e do SQL Server.

## Ponto flutuante e dinheiro

```csharp
Temperature = SqlParam.Real(23.5F)
Ratio = SqlParam.Float(0.75D)
Price = SqlParam.Money(12.34M)
Fee = SqlParam.SmallMoney(1.23M)
```

Use essas factories somente quando o contrato do banco usar o tipo SQL Server
correspondente.

## Valores null

```csharp
Discount = SqlParam.Decimal(null, precision: 9, scale: 4)
```

`null` é materializado como `DBNull.Value`.
