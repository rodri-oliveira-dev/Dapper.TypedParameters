# Dapper.TypedParameters

`Dapper.TypedParameters.SqlServer` permite declarar explicitamente tipos de parametros SQL Server usados pelo Dapper.

O primeiro preview foca em parametros SQL Server explicitos com `Microsoft.Data.SqlClient`. A ideia e deixar tipo, tamanho, precisao e escala visiveis no ponto de chamada, em vez de depender apenas da inferencia padrao do provider.

## Motivacao

Ao enviar uma `string` .NET para o SQL Server, o parametro pode ser inferido como `nvarchar`. Se a coluna comparada for `varchar`, o SQL Server pode precisar aplicar conversoes implicitas durante a execucao.

Dependendo da consulta, dos tipos envolvidos, da collation e dos indices disponiveis, essas conversoes podem afetar o plano de execucao e reduzir a capacidade do otimizador de usar uma busca eficiente. Este pacote ajuda o chamador a declarar `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)` ou `nvarchar(max)` de forma explicita.

Declarar o tipo correto do parametro nao promete eliminar todas as conversoes implicitas de uma consulta. O SQL, o schema, expressoes, funcoes, collations e outros parametros continuam fazendo parte do plano final.

Para parametros numericos, o pacote tambem evita que precisao e escala de `decimal` fiquem implicitas no ponto de chamada. A biblioteca nao arredonda valores; conversoes validas continuam sob responsabilidade de `Microsoft.Data.SqlClient` e do SQL Server.

Para parametros binarios e identificadores, o pacote declara `uniqueidentifier`, `binary`, `varbinary` e `varbinary(max)` sem copiar arrays, inferir tamanho a partir do valor ou transformar arrays vazios em `null`.

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
SqlParam.UniqueIdentifier(value)
SqlParam.Binary(value, size)
SqlParam.VarBinary(value, size)
SqlParam.VarBinaryMax(value)
SqlParam.Date(value)
SqlParam.Time(value, scale)
SqlParam.DateTime(value)
SqlParam.SmallDateTime(value)
SqlParam.DateTime2(value, scale)
SqlParam.DateTimeOffset(value, scale)
SqlParam.TableValued(typeName, value)
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

### Parametros binarios e identificadores

```csharp
using System;
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class BinaryExample
{
    public static async Task<int> SaveFileAsync(
        string connectionString,
        Guid fileId,
        byte[] checksum,
        byte[] payload)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            """
            INSERT INTO dbo.Files (FileId, Checksum, Payload)
            VALUES (@FileId, @Checksum, @Payload);
            """,
            new
            {
                FileId = SqlParam.UniqueIdentifier(fileId),
                Checksum = SqlParam.Binary(checksum, 32),
                Payload = SqlParam.VarBinaryMax(payload)
            });
    }
}
```

### Parametros temporais

```csharp
using System;
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class TemporalExample
{
    public static async Task<int> InsertEventAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        return await connection.ExecuteAsync(
            """
            INSERT INTO dbo.Events (EventDate, EventTime, PublishedAt)
            VALUES (@EventDate, @EventTime, @PublishedAt);
            """,
            new
            {
                EventDate = SqlParam.Date(new DateOnly(2026, 8, 5)),
                EventTime = SqlParam.Time(new TimeOnly(12, 30), scale: 0),
                PublishedAt = SqlParam.DateTime2(
                    new DateTime(2026, 8, 5, 12, 30, 0),
                    scale: 7)
            });
    }
}
```

### Stored procedure com output

Stored procedure:

```sql
CREATE PROCEDURE dbo.CreateCustomer
    @Name nvarchar(100),
    @CustomerId int OUTPUT,
    @ExternalId uniqueidentifier OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @CustomerId = 42;
    SET @ExternalId = NEWID();
END
```

Chamada com Dapper:

```csharp
using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class OutputExample
{
    public static async Task<(int CustomerId, Guid ExternalId)> CreateCustomerAsync(
        string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        var customerId = SqlParam.Int(null).AsOutput();
        var externalId = SqlParam.UniqueIdentifier(null).AsOutput();

        await connection.ExecuteAsync(
            "dbo.CreateCustomer",
            new
            {
                Name = SqlParam.NVarChar("Ada Lovelace", 100),
                CustomerId = customerId,
                ExternalId = externalId
            },
            commandType: CommandType.StoredProcedure);

        return (
            customerId.GetValue<int>(),
            externalId.GetValue<Guid>());
    }
}
```

### Stored procedure com input/output

Stored procedure:

```sql
CREATE PROCEDURE dbo.IncrementCounter
    @Counter int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Counter = @Counter + 1;
END
```

Chamada com Dapper:

```csharp
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class InputOutputExample
{
    public static async Task<int> IncrementAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        var counter = SqlParam.Int(1).AsInputOutput();

        await connection.ExecuteAsync(
            "dbo.IncrementCounter",
            new { Counter = counter },
            commandType: CommandType.StoredProcedure);

        return counter.GetValue<int>();
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
| `binary` | 1 a 8.000 bytes declarados |
| `varbinary` | 1 a 8.000 bytes declarados |
| `varbinary(max)` | Use `SqlParam.VarBinaryMax(value)` |

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

Tipos binarios e identificadores suportados:

| Tipo SQL Server | Factory |
| --- | --- |
| `uniqueidentifier` | `SqlParam.UniqueIdentifier(value)` |
| `binary` | `SqlParam.Binary(value, size)` |
| `varbinary` | `SqlParam.VarBinary(value, size)` |
| `varbinary(max)` | `SqlParam.VarBinaryMax(value)` |

`SqlParam.Binary` e `SqlParam.VarBinary` aceitam `size` de 1 a 8.000. `SqlParam.VarBinaryMax` configura `Size = -1`. Arrays vazios sao preservados como arrays vazios; somente `null` e convertido para `DBNull.Value` quando o parametro e materializado. A biblioteca nao copia arrays, nao valida `value.Length <= size` e nao trunca conteudo.

Tipos temporais suportados:

| Tipo SQL Server | Factory |
| --- | --- |
| `date` | `SqlParam.Date(value)` |
| `time` | `SqlParam.Time(value, scale)` |
| `datetime` | `SqlParam.DateTime(value)` |
| `smalldatetime` | `SqlParam.SmallDateTime(value)` |
| `datetime2` | `SqlParam.DateTime2(value, scale)` |
| `datetimeoffset` | `SqlParam.DateTimeOffset(value, scale)` |

`time`, `datetime2` e `datetimeoffset` aceitam `scale` de 0 a 7. A biblioteca nao normaliza timezone, nao altera `DateTime.Kind`, nao arredonda manualmente e nao valida toda a faixa de datas do SQL Server.

## Tratamento de null

Ao criar o `SqlParameter`, `null` e convertido para `DBNull.Value`. Isso acontece quando o Dapper aplica o parametro ao comando SQL Server.

## Output parameters

Qualquer parametro escalar pode ser configurado como `Output` ou `InputOutput`
com os metodos fluentes:

```csharp
var result = SqlParam.VarChar(null, 100).AsOutput();
var counter = SqlParam.Int(1).AsInputOutput();
```

Guarde a mesma instancia passada ao Dapper. Depois que `Execute` ou
`ExecuteAsync` terminar, leia o valor com `OutputValue` ou `GetValue<T>()`.

`OutputValue` normaliza `DBNull.Value` para `null`. `GetValue<T>()` tambem
retorna `null` para tipos de referencia e `Nullable<T>`. Para value types nao
nullable, `DBNull.Value` gera `InvalidOperationException`; o metodo nao retorna
`default` silenciosamente.

`GetValue<T>()` usa regras normais de cast CLR e nao faz conversoes silenciosas,
parsing ou `Convert.ChangeType`. Pedir um tipo incompativel gera
`InvalidCastException`.

A mesma instancia de parametro output nao deve ser usada concorrentemente em
comandos diferentes. Reutilizacao nao concorrente e permitida; a leitura passa a
refletir o `SqlParameter` materializado mais recentemente.

Leia outputs somente apos a conclusao da execucao do comando. Se a instancia
ainda nao tiver sido materializada pelo Dapper, a leitura gera
`InvalidOperationException`.

## Table-valued parameters

TVPs usam um user-defined table type existente no SQL Server. A biblioteca
configura `SqlDbType.Structured`, `TypeName`, `Value` e
`ParameterDirection.Input`; ela nao cria o tipo no banco e nao compara o schema
do `DataTable` com o table type.

Exemplo de table type:

```sql
CREATE TYPE dbo.CustomerBatch AS TABLE
(
    CustomerId int NOT NULL,
    Name nvarchar(100) NOT NULL,
    IsActive bit NOT NULL
);
```

Montagem do `DataTable` e chamada pelo Dapper:

```csharp
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static class TableValuedExample
{
    public static async Task<int> InsertCustomersAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);

        using var customers = new DataTable();
        customers.Columns.Add("CustomerId", typeof(int));
        customers.Columns.Add("Name", typeof(string));
        customers.Columns.Add("IsActive", typeof(bool));
        customers.Rows.Add(1, "Ada Lovelace", true);
        customers.Rows.Add(2, "Grace Hopper", true);

        return await connection.ExecuteAsync(
            """
            INSERT INTO dbo.Customers (CustomerId, Name, IsActive)
            SELECT CustomerId, Name, IsActive
            FROM @Customers;
            """,
            new
            {
                Customers = SqlParam.TableValued(
                    "dbo.CustomerBatch",
                    customers)
            });
    }
}
```

Um `DataTable` sem linhas e aceito, desde que as colunas tenham sido declaradas
para corresponder ao table type. `DataTable` nulo e rejeitado. TVPs sao
input-only nesta API; nao ha suporte a `Output`, `InputOutput`, `Size`,
`Precision` ou `Scale`.

O schema e responsabilidade do chamador. A biblioteca nao consulta o banco, nao
infere colunas, nao mapeia POCOs e nao valida automaticamente se as colunas do
`DataTable` correspondem ao user-defined table type. Incompatibilidades sao
validadas pelo `Microsoft.Data.SqlClient` ou pelo SQL Server durante a execucao.

## O que o pacote nao faz

- Nao consulta schema.
- Nao analisa plano de execucao.
- Nao detecta automaticamente `CONVERT_IMPLICIT`.
- Nao altera SQL.
- Nao trata listas `IN`.
- Nao infere schema de TVP.
- Nao mapeia POCOs para TVP.
- Nao oferece `image`, `rowversion`, `timestamp` ou `filestream` nesta versao.
- Nao oferece outros providers nesta versao.
- Nao valida se o conteudo cabe em bytes no tamanho declarado.

`rowversion` e `timestamp` nao sao tipos de entrada comuns e estao fora do escopo atual.

## Testes e build

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build
dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
```

O `pack` gera o pacote NuGet `.nupkg` e o pacote de simbolos `.snupkg`. O
pacote usa SourceLink para GitHub, metadata de repositorio, README empacotado,
licenca MIT por expressao e XML documentation para `net8.0` e `net10.0`.

Para validar localmente o conteudo do pacote gerado:

```bash
pwsh ./scripts/Test-PackageContents.ps1 -PackageDirectory ./artifacts/packages
```

Essa validacao confere assets para `net8.0` e `net10.0`, XML documentation,
README, licenca, repository URL, dependencias, simbolos, SourceLink basico e
ausencia de DLLs de teste, arquivos `bin`/`obj`, temporarios e padroes obvios
de segredo.

Mudancas acidentais na API publica sao verificadas por PublicApiAnalyzers em:

```text
src/Dapper.TypedParameters.SqlServer/PublicAPI.Shipped.txt
src/Dapper.TypedParameters.SqlServer/PublicAPI.Unshipped.txt
```

Ao alterar intencionalmente a API publica, atualize esses arquivos na mesma
mudanca e documente a decisao.

Para gerar cobertura local:

```bash
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults/coverage/unit
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults/coverage/integration
```

Nao ha threshold de cobertura neste preview; primeiro a cobertura atual deve
ser medida e revisada.

Os testes de integracao usam SQL Server real em container por meio de `Testcontainers.MsSql` e da imagem oficial `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04`. Para executa-los, Docker precisa estar disponivel e apto a iniciar containers Linux de SQL Server. A suite falha quando Docker ou SQL Server nao estao disponiveis.

## Benchmarks

Benchmarks locais ficam em:

```text
benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks/
```

Eles usam BenchmarkDotNet e cobrem criacao de parametros, materializacao em
`SqlCommand`, string, decimal, binario e TVP pequeno. Eles nao exigem SQL Server.

Para compilar:

```bash
dotnet build benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks.csproj --configuration Release
```

Para executar manualmente:

```bash
dotnet run --project benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks/Dapper.TypedParameters.SqlServer.Benchmarks.csproj --configuration Release --framework net8.0 -- --filter '*'
```

Benchmarks completos nao rodam automaticamente em pull requests.

## Roadmap

Itens planejados como trabalho futuro, sem API publica nesta versao:

- Outros providers.

## Afiliacao

Este projeto nao e afiliado, mantido ou endossado oficialmente pelo projeto Dapper ou pela Microsoft.

## Licenca

Este projeto e licenciado sob a licenca MIT.
