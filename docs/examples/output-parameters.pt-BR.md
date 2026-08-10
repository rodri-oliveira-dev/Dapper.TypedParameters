# Parâmetros de Saída

[English](output-parameters.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Parâmetros escalares podem ser configurados como `Output` ou `InputOutput` com
métodos fluentes. Table-valued parameters são input-only nesta API.

## Output

Stored procedure:

```sql
CREATE PROCEDURE dbo.CreateCustomer
    @Name nvarchar(100),
    @CustomerId int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @CustomerId = 42;
END
```

Chamada Dapper:

```csharp
using System.Data;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var customerId = SqlParam.Int(null).AsOutput();

await connection.ExecuteAsync(
    "dbo.CreateCustomer",
    new
    {
        Name = SqlParam.NVarChar("Ada Lovelace", 100),
        CustomerId = customerId
    },
    commandType: CommandType.StoredProcedure);

int id = customerId.GetValue<int>();
```

Guarde a mesma instância de parâmetro passada ao Dapper. Leia `OutputValue` ou
`GetValue<T>()` somente depois que `Execute` ou `ExecuteAsync` terminar.

## InputOutput

```csharp
var counter = SqlParam.Int(41).AsInputOutput();

await connection.ExecuteAsync(
    "dbo.IncrementCounter",
    new { Counter = counter },
    commandType: CommandType.StoredProcedure);

int next = counter.GetValue<int>();
```

## DBNull.Value

`OutputValue` normaliza `DBNull.Value` para `null`.

```csharp
string? value = output.GetValue<string?>();
```

Para value types não nullable, `GetValue<T>()` lança exceção se o valor do banco
for null. Ele não retorna `default` silenciosamente.

`GetValue<T>()` usa regras normais de cast CLR. Ele não faz parsing de strings
nem chama `Convert.ChangeType`.

## Concorrência e reuso

Instâncias de parâmetro output retêm internamente o `SqlParameter` materializado
mais recente. Reuso é suportado para comandos não concorrentes, mas a mesma
instância output não deve ser compartilhada concorrentemente entre comandos.
