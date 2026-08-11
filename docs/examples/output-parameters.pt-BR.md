# Parâmetros de Saída

[English](output-parameters.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Parâmetros escalares podem ser configurados como `Output` ou `InputOutput` com
métodos fluentes. Table-valued parameters são input-only nesta API.

## Lifecycle

```text
criar parâmetro
  -> passar a mesma instância ao Dapper
  -> executar o comando
  -> ler OutputValue ou GetValue<T>()
```

Leia valores de saída somente depois que o Dapper materializar o parâmetro e a
execução do comando terminar. Ler antes da materialização lança
`InvalidOperationException`.

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

## InputOutput

```csharp
var counter = SqlParam.Int(41).AsInputOutput();

await connection.ExecuteAsync(
    "dbo.IncrementCounter",
    new { Counter = counter },
    commandType: CommandType.StoredProcedure);

int next = counter.GetValue<int>();
```

## OutputValue

```csharp
object? raw = customerId.OutputValue;
```

`OutputValue` retorna o valor materializado pelo provider depois da execução.
`DBNull.Value` é normalizado para `null`.

## GetValue<T>()

```csharp
int id = customerId.GetValue<int>();
string? optionalCode = code.GetValue<string?>();
```

`GetValue<T>()` usa regras normais de cast CLR. Ele não faz parsing de strings,
não chama `Convert.ChangeType` e não retorna `default` silenciosamente para null
do banco atribuído a um value type não nullable. Casts incompatíveis lançam
`InvalidCastException`.

## Reuso

Instâncias de parâmetros output retêm internamente o `SqlParameter` materializado
mais recente. Reuso é suportado para comandos não concorrentes, mas a mesma
instância output não deve ser compartilhada concorrentemente entre comandos.
