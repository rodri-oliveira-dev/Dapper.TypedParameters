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

## Unicode e Não Unicode

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

Escolha `varchar` ou `nvarchar` de acordo com o schema ou o contrato da stored
procedure. A biblioteca não trata um como inerentemente melhor ou mais rápido.

## Tamanho Fixo e Variável

```csharp
StateCode = SqlParam.Char("SP", 2)
LanguageCode = SqlParam.NChar("PT", 2)
Nickname = SqlParam.VarChar("ada", 40)
DisplayName = SqlParam.NVarChar("Ada Lovelace", 100)
```

A biblioteca declara o tipo SQL fixo ou variável e o tamanho solicitado. SQL
Server e `Microsoft.Data.SqlClient` controlam armazenamento, padding e conversões
durante a execução.

## Tipos MAX

```csharp
AnsiPayload = SqlParam.VarCharMax(payload)
UnicodePayload = SqlParam.NVarCharMax(payload)
```

Use as factories explícitas de `max` em vez de passar `-1` para factories com
tamanho limitado.

## Valores Nulos

```csharp
Nickname = SqlParam.NVarChar(null, 80)
```

`null` é convertido para `DBNull.Value` quando o parâmetro é materializado.

## Observações Sobre Tamanho

Para `varchar` e `char`, tamanhos SQL Server são declarados em bytes. A relação
entre bytes e caracteres depende dos dados e da configuração do SQL Server. A
biblioteca valida o intervalo do tamanho declarado, mas não valida se um valor
específico cabe em bytes.
