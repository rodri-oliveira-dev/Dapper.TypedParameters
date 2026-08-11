# Parâmetros String

[English](strings.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Factories de string declaram metadados SQL Server de string explicitamente.

| Factory | Tipo SQL Server | Tamanho |
| --- | --- | --- |
| `SqlParam.VarChar(value, size)` | `varchar(size)` | 1 a 8.000 |
| `SqlParam.NVarChar(value, size)` | `nvarchar(size)` | 1 a 4.000 |
| `SqlParam.Char(value, size)` | `char(size)` | 1 a 8.000 |
| `SqlParam.NChar(value, size)` | `nchar(size)` | 1 a 4.000 |
| `SqlParam.VarCharMax(value)` | `varchar(max)` | `Size = -1` |
| `SqlParam.NVarCharMax(value)` | `nvarchar(max)` | `Size = -1` |

## varchar e nvarchar

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

A biblioteca não presume que `varchar` é melhor que `nvarchar`. Escolha a
factory que corresponde ao schema ou ao contrato da stored procedure.

## Strings de tamanho fixo

```csharp
StateCode = SqlParam.Char("SP", 2)
LanguageCode = SqlParam.NChar("A", 1)
```

A biblioteca declara o tipo SQL fixo e o tamanho. SQL Server e
`Microsoft.Data.SqlClient` controlam o comportamento de armazenamento de tamanho
fixo.

## Tipos max

```csharp
AnsiPayload = SqlParam.VarCharMax(payload)
UnicodePayload = SqlParam.NVarCharMax(payload)
```

Use as factories explícitas de `max` em vez de passar `-1` para factories com
tamanho limitado.

## Valores null

```csharp
Nickname = SqlParam.NVarChar(null, 80)
```

`null` é materializado como `DBNull.Value`.

## Observações sobre tamanho

Para `varchar` e `char`, tamanhos SQL Server são declarados em bytes. A relação
entre bytes e caracteres depende dos dados e da configuração do SQL Server. A
biblioteca valida o intervalo do tamanho declarado, mas não valida se um valor
específico cabe em bytes.
