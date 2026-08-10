# Dapper.TypedParameters

[English](README.md) | Português (Brasil)

`Dapper.TypedParameters.SqlServer` fornece parâmetros SQL Server explicitamente
tipados para Dapper usando `Microsoft.Data.SqlClient`. A biblioteca ajuda o
chamador a declarar os metadados do parâmetro enviados ao provider, como tipo,
tamanho, precisão, escala, direção e nome de tipo para table-valued parameters.

## Por quê?

Dapper torna o envio de parâmetros conveniente:

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT *
    FROM Customers
    WHERE Document = @Document
    """,
    new
    {
        Document = "12345678901"
    });
```

Nesse exemplo, `Document` é uma `string` .NET. O provider SQL Server precisa
materializar esse valor como um parâmetro SQL. Dependendo do schema, da query e
do caminho usado pelo provider, os metadados do parâmetro materializado podem
não corresponder exatamente à definição da coluna, por exemplo:

```sql
Document varchar(11)
```

Esta biblioteca permite que o chamador expresse essa intenção explicitamente
quando conhece o tipo esperado pelo banco.

## O problema

Inferência de parâmetros é útil e correta em muitos cenários com Dapper. O
trade-off é que o tipo SQL enviado ao SQL Server nem sempre fica visível no ponto
de chamada. Quando os metadados do parâmetro e da coluna diferem, o SQL Server
pode precisar realizar conversões implícitas ao avaliar uma query.

Essas conversões podem importar dependendo dos tipos envolvidos, da precedência
de tipos, da collation, do formato da query, dos índices e do plano de execução
final. Esta biblioteca não garante queries mais rápidas. Ela dá controle ao
chamador sobre os metadados SQL enviados ao SQL Server.

## Antes

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT
        Id,
        Document,
        Name
    FROM Customers
    WHERE Document = @Document
    """,
    new
    {
        Document = "12345678901"
    });
```

Esse código é Dapper idiomático. Ele apenas não declara que o schema espera um
parâmetro `varchar(11)`.

## Com parâmetros tipados

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT
        Id,
        Document,
        Name
    FROM Customers
    WHERE Document = @Document
    """,
    new
    {
        Document = SqlParam.VarChar("12345678901", 11)
    });
```

```text
.NET string
  -> metadados SQL explícitos
  -> varchar(11)
```

`SqlParam.VarChar(...)` não é escolhido automaticamente pela biblioteca. O
desenvolvedor escolhe esse tipo porque conhece o schema do banco.

Colunas Unicode também devem ser declaradas explicitamente:

```csharp
Name = SqlParam.NVarChar("João", 150)
```

A biblioteca não presume que `varchar` é melhor que `nvarchar`. A proposta é
correspondência explícita com o schema, não preferência por um tipo SQL
específico.

## Instalação

Instale pelo NuGet.org depois que o pacote for publicado:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 0.1.0-preview.1
```

Para testar uma build local ainda não publicada a partir deste repositório:

```bash
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --output ./artifacts/packages
dotnet add package TypedParameters.Dapper.SqlServer --version 0.1.0-preview.1 --source ./artifacts/packages
```

Package ID NuGet:

```text
TypedParameters.Dapper.SqlServer
```

O pacote tem uma identidade NuGet distinta da identidade do assembly e do
namespace C#:

```text
Pacote NuGet: TypedParameters.Dapper.SqlServer
Assembly: Dapper.TypedParameters.SqlServer.dll
Namespace: Dapper.TypedParameters.SqlServer
```

Se o NuGet.org ainda não contiver a versão solicitada, use as instruções de
build local acima ou aguarde o workflow de release publicar essa versão.

## Quick start

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

public static async Task<Customer?> FindCustomerAsync(
    string connectionString,
    string document)
{
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();

    return await connection.QuerySingleOrDefaultAsync<Customer>(
        """
        SELECT
            Id,
            Document,
            Name
        FROM dbo.Customers
        WHERE Document = @Document;
        """,
        new
        {
            Document = SqlParam.VarChar(document, 11)
        });
}
```

## Tipos de parâmetros suportados

| Família | Tipos SQL Server |
| --- | --- |
| Strings | `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)`, `nvarchar(max)` |
| Numéricos | `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money`, `smallmoney` |
| Binários e identificadores | `uniqueidentifier`, `binary`, `varbinary`, `varbinary(max)` |
| Temporais | `date`, `time`, `datetime`, `smalldatetime`, `datetime2`, `datetimeoffset` |
| Output / InputOutput | `AsOutput()` e `AsInputOutput()` fluentes em parâmetros escalares |
| Table-valued parameters | `SqlDbType.Structured` com `TypeName` explícito e `DataTable` |

## Compatibilidade

| Item | Suporte |
| --- | --- |
| Target frameworks | `net8.0`; `net10.0` |
| Dapper | `2.1.79` |
| Microsoft.Data.SqlClient | `6.1.6` |
| Provider ADO.NET | Somente `Microsoft.Data.SqlClient` |
| System.Data.SqlClient | Não suportado |
| Alvo declarado de compatibilidade do driver | SQL Server 2016 até SQL Server 2025 |
| SQL Server testado pela CI | SQL Server 2022 via `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04` |
| Azure SQL Database | Compatível pelo driver; não testado por integração neste repositório |
| Azure SQL Managed Instance | Compatível pelo driver; não testado por integração neste repositório |
| Azure Synapse Analytics | Compatível pelo driver; não testado por integração neste repositório |

As entradas de SQL Server e Azure SQL acima descrevem compatibilidade do driver
`Microsoft.Data.SqlClient`. Este repositório atualmente executa testes de
integração apenas na imagem SQL Server 2022 listada na tabela.

## Por que tipos SQL explícitos podem importar

SQL Server avalia expressões usando metadados de tipos SQL, não apenas valores
CLR. Metadados explícitos dão ao chamador controle sobre o tipo SQL enviado ao
SQL Server e podem ajudar a evitar divergências de tipo quando o tipo esperado
do banco é conhecido.

Isso é mais útil quando código e schema estão alinhados intencionalmente, como:

- identificadores `varchar(11)`;
- nomes `nvarchar(150)`;
- valores monetários `decimal(18, 2)`;
- valores `time(0)`, `datetime2(7)` ou `datetimeoffset(7)`;
- parâmetros de saída de stored procedures;
- user-defined table types para TVPs.

Meça queries sensíveis a performance no seu próprio workload. Este pacote torna
a intenção explícita; ele não analisa nem otimiza planos de execução.

## O que esta biblioteca não faz

A biblioteca não:

- inspeciona o schema do banco;
- reescreve SQL;
- analisa planos de execução;
- detecta automaticamente `CONVERT_IMPLICIT`;
- escolhe automaticamente o tipo SQL correto;
- substitui o sistema de parâmetros do Dapper;
- altera o schema do banco;
- gerencia índices;
- otimiza queries arbitrárias;
- valida definições de colunas do banco.

`SqlParam.VarChar(value, 11)` é uma declaração explícita feita pelo chamador. A
biblioteca não sabe se a coluna de destino realmente é `varchar(11)`.

## Documentação

- [Primeiros passos](docs/getting-started.pt-BR.md)
- [Motivação](docs/motivation.pt-BR.md)
- Exemplos:
  - [Strings](docs/examples/strings.pt-BR.md)
  - [Numéricos](docs/examples/numeric.pt-BR.md)
  - [Binários e identificadores](docs/examples/binary.pt-BR.md)
  - [Temporais](docs/examples/temporal.pt-BR.md)
  - [Parâmetros de saída](docs/examples/output-parameters.pt-BR.md)
  - [Table-valued parameters](docs/examples/table-valued-parameters.pt-BR.md)
- [English](README.md)

## Testes

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
```

Os testes de integração usam SQL Server via Docker e `Testcontainers.MsSql`.

## Contribuindo

Issues e pull requests são bem-vindos. Mantenha mudanças pequenas, explícitas e
validadas para os dois target frameworks suportados.

## Licença

Este projeto é licenciado sob a licença MIT.

## Aviso

Este projeto não é afiliado, mantido ou endossado oficialmente pelo projeto
Dapper ou pela Microsoft.
