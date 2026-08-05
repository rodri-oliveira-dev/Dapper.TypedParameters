# Dapper.TypedParameters

`Dapper.TypedParameters.SqlServer` permite declarar explicitamente tipos de parametros SQL Server usados pelo Dapper.

O primeiro preview foca em parametros SQL Server explicitos com `Microsoft.Data.SqlClient`. A ideia e deixar tipo, tamanho, precisao e escala visiveis no ponto de chamada, em vez de depender apenas da inferencia padrao do provider.

## Motivacao

Ao enviar uma `string` .NET para o SQL Server, o parametro pode ser inferido como `nvarchar`. Se a coluna comparada for `varchar`, o SQL Server pode precisar aplicar conversoes implicitas durante a execucao.

Dependendo da consulta, dos tipos envolvidos, da collation e dos indices disponiveis, essas conversoes podem afetar o plano de execucao e reduzir a capacidade do otimizador de usar uma busca eficiente. Este pacote ajuda o chamador a declarar `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)` ou `nvarchar(max)` de forma explicita.

Declarar o tipo correto do parametro nao promete eliminar todas as conversoes implicitas de uma consulta. O SQL, o schema, expressoes, funcoes, collations e outros parametros continuam fazendo parte do plano final.

Para parametros numericos, o pacote tambem evita que precisao e escala de `decimal` fiquem implicitas no ponto de chamada. A biblioteca nao arredonda valores; conversoes validas continuam sob responsabilidade de `Microsoft.Data.SqlClient` e do SQL Server.

## Compatibilidade

| Item | Suporte |
| --- | --- |
| .NET | `net8.0`; `net10.0` |
| Dapper | `2.1.79` |
| Microsoft.Data.SqlClient | `7.0.2` |
| SQL Server | Suportado |
| Provider ADO.NET | Somente `Microsoft.Data.SqlClient` |

Um unico pacote contem assets para `net8.0` e `net10.0`. A API publica e igual nos dois TFMs.

Somente `Microsoft.Data.SqlClient` e suportado. `System.Data.SqlClient` nao e suportado nesta biblioteca.

Os limites de tipos SQL Server nao mudam conforme o TFM. `varchar`, `nvarchar`, `char`, `nchar` e tipos `max` seguem os limites declarados pelo SQL Server, nao pelo framework alvo.

## Instalacao

O pacote ainda nao foi publicado publicamente. Para validar localmente a partir deste repositorio, gere o pacote e instale usando uma fonte local:

```bash
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --output ./artifacts/packages
dotnet add package Dapper.TypedParameters.SqlServer --version 0.1.0-preview.1 --source ./artifacts/packages
```

Package ID planejado para o preview:

```text
Dapper.TypedParameters.SqlServer
```

O nome definitivo do pacote depende da validacao de disponibilidade e das regras de prefixo reservado do NuGet.

## API atual

```csharp
SqlParam.VarChar(value, size)
SqlParam.NVarChar(value, size)
SqlParam.Char(value, size)
SqlParam.NChar(value, size)
SqlParam.VarCharMax(value)
SqlParam.NVarCharMax(value)
SqlParam.Bit(value)
SqlParam.TinyInt(value)
SqlParam.SmallInt(value)
SqlParam.Int(value)
SqlParam.BigInt(value)
SqlParam.Real(value)
SqlParam.Float(value)
SqlParam.Decimal(value, precision, scale)
SqlParam.Money(value)
SqlParam.SmallMoney(value)
```

Todos os metodos retornam um parametro que o Dapper consome como `SqlMapper.ICustomQueryParameter`.

## Exemplos completos

### SELECT

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class SelectExample
{
    public static async Task<string> FindDocumentAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.QuerySingleAsync<string>(
            "SELECT CAST(@Document AS varchar(11));",
            new
            {
                Document = SqlParam.VarChar("12345678901", 11)
            });
    }
}
```

### INSERT com mais de um parametro

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class InsertExample
{
    public static async Task<int> InsertCustomerAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            """
            INSERT INTO dbo.Customers (Document, Name)
            VALUES (@Document, @Name);
            """,
            new
            {
                Document = SqlParam.VarChar("12345678901", 11),
                Name = SqlParam.NVarChar("Ada Lovelace", 100)
            });
    }
}
```

### UPDATE

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class UpdateExample
{
    public static async Task<int> UpdateCustomerStatusAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            """
            UPDATE dbo.Customers
            SET Status = @Status
            WHERE Document = @Document;
            """,
            new
            {
                Document = SqlParam.VarChar("12345678901", 11),
                Status = SqlParam.NVarChar("Approved", 30)
            });
    }
}
```

### DELETE

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class DeleteExample
{
    public static async Task<int> DeleteCustomerAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            "DELETE FROM dbo.Customers WHERE Document = @Document;",
            new
            {
                Document = SqlParam.VarChar("12345678901", 11)
            });
    }
}
```

### Valor null

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class NullExample
{
    public static async Task<int> ClearNicknameAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            """
            UPDATE dbo.Customers
            SET Nickname = @Nickname
            WHERE Document = @Document;
            """,
            new
            {
                Document = SqlParam.VarChar("12345678901", 11),
                Nickname = SqlParam.NVarChar(null, 80)
            });
    }
}
```

### varchar(max)

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class VarCharMaxExample
{
    public static async Task<int> SaveAnsiPayloadAsync(
        string connectionString,
        string payload)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            "INSERT INTO dbo.Payloads (AnsiPayload) VALUES (@Payload);",
            new
            {
                Payload = SqlParam.VarCharMax(payload)
            });
    }
}
```

### nvarchar(max)

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class NVarCharMaxExample
{
    public static async Task<int> SaveUnicodePayloadAsync(
        string connectionString,
        string payload)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            "INSERT INTO dbo.Payloads (UnicodePayload) VALUES (@Payload);",
            new
            {
                Payload = SqlParam.NVarCharMax(payload)
            });
    }
}
```

### Chamada assincrona

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class AsyncExample
{
    public static async Task<int> CountCustomersByStateAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM dbo.Customers WHERE StateCode = @StateCode;",
            new
            {
                StateCode = SqlParam.Char("SP", 2)
            });
    }
}
```

### Parametros numericos

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class NumericExample
{
    public static async Task<int> InsertInvoiceAsync(
        string connectionString,
        int customerId,
        decimal amount)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            """
            INSERT INTO dbo.Invoices (CustomerId, Amount, IsPaid)
            VALUES (@CustomerId, @Amount, @IsPaid);
            """,
            new
            {
                CustomerId = SqlParam.Int(customerId),
                Amount = SqlParam.Decimal(amount, precision: 18, scale: 2),
                IsPaid = SqlParam.Bit(false)
            });
    }
}
```

### WHERE com parametro numerico

```csharp
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class NumericWhereExample
{
    public static async Task<decimal> GetBalanceAsync(
        string connectionString,
        long accountId)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.QuerySingleAsync<decimal>(
            "SELECT Balance FROM dbo.Accounts WHERE AccountId = @AccountId;",
            new
            {
                AccountId = SqlParam.BigInt(accountId)
            });
    }
}
```

## Limites

| Tipo SQL Server | Tamanho aceito |
| --- | --- |
| `varchar` / `char` | 1 a 8.000 bytes declarados |
| `nvarchar` / `nchar` | 1 a 4.000 unidades declaradas |
| `varchar(max)` | Use `SqlParam.VarCharMax(value)` |
| `nvarchar(max)` | Use `SqlParam.NVarCharMax(value)` |

Para `varchar` e `char`, o tamanho declarado pode representar bytes, nao uma equivalencia universal com quantidade de caracteres. A relacao entre bytes e caracteres depende dos dados e da configuracao do SQL Server.

Os metodos com `size` rejeitam valores fora dos intervalos acima com `ArgumentOutOfRangeException`. Para tipos `max`, use os metodos proprios em vez de passar `-1`.

Tipos numericos e booleanos suportados:

| Tipo SQL Server | Factory |
| --- | --- |
| `bit` | `SqlParam.Bit(value)` |
| `tinyint` | `SqlParam.TinyInt(value)` |
| `smallint` | `SqlParam.SmallInt(value)` |
| `int` | `SqlParam.Int(value)` |
| `bigint` | `SqlParam.BigInt(value)` |
| `real` | `SqlParam.Real(value)` |
| `float` | `SqlParam.Float(value)` |
| `decimal` | `SqlParam.Decimal(value, precision, scale)` |
| `money` | `SqlParam.Money(value)` |
| `smallmoney` | `SqlParam.SmallMoney(value)` |

`SqlParam.Decimal` aceita `precision` de 1 a 38 e `scale` de 0 ate `precision`. Valores fora desses limites sao rejeitados com `ArgumentOutOfRangeException`.

Nao ha factory `SqlParam.Numeric`: no SQL Server, `numeric` e sinonimo de `decimal`. Tambem nao ha overload generico como `SqlParam.Number<T>`, porque a API exige declaracao explicita do tipo SQL Server.

## Tratamento de null

Ao criar o `SqlParameter`, `null` e convertido para `DBNull.Value`. Isso acontece quando o Dapper aplica o parametro ao comando SQL Server.

## O que o pacote nao faz

- Nao consulta schema.
- Nao analisa plano de execucao.
- Nao detecta automaticamente `CONVERT_IMPLICIT`.
- Nao altera SQL.
- Nao trata listas `IN`.
- Nao oferece output parameters nesta versao.
- Nao oferece TVPs nesta versao.
- Nao oferece outros providers nesta versao.
- Nao valida se o conteudo cabe em bytes no tamanho declarado.

## Testes e build

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build
dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
```

Os testes de integracao usam SQL Server real em container por meio de `Testcontainers.MsSql` e da imagem oficial `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04`. Para executa-los, Docker precisa estar disponivel e apto a iniciar containers Linux de SQL Server. A suite falha quando Docker ou SQL Server nao estao disponiveis.

## Roadmap

Itens planejados como trabalho futuro, sem API publica nesta versao:

- Tipos numericos.
- Tipos binarios.
- Datas e horarios.
- Output parameters.
- TVPs.
- Outros providers.

## Afiliacao

Este projeto nao e afiliado, mantido ou endossado oficialmente pelo projeto Dapper ou pela Microsoft.

## Licenca

Este projeto e licenciado sob a licenca MIT.
