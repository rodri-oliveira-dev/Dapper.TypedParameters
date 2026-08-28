# Dapper.TypedParameters

[English](README.md) | Português (Brasil)

[![NuGet](https://img.shields.io/nuget/v/TypedParameters.Dapper.SqlServer?logo=nuget&label=NuGet)](https://www.nuget.org/packages/TypedParameters.Dapper.SqlServer)
[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=rodri-oliveira-dev_Dapper.TypedParameters&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rodri-oliveira-dev_Dapper.TypedParameters)
[![CI](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters)

`Dapper.TypedParameters` hospeda pacotes específicos por provider para metadados
explícitos de parâmetros de banco de dados no Dapper.

Pacotes disponíveis:

- `TypedParameters.Dapper.SqlServer`
- `TypedParameters.Dapper.PostgreSql` (fundação estrutural; factories públicas
  ainda não implementadas)

`Dapper.TypedParameters.SqlServer` fornece metadados explícitos de parâmetros SQL
Server usando `Microsoft.Data.SqlClient`.

Use quando o contrato do banco é conhecido e o tipo SQL Server, tamanho,
precisão, escala, direção ou nome do table-valued parameter deve ficar visível
no ponto de chamada.

## Instalação

Instale o pacote stable atual pelo NuGet.org:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

Página oficial do pacote:
[TypedParameters.Dapper.SqlServer no NuGet.org](https://www.nuget.org/packages/TypedParameters.Dapper.SqlServer/1.0.0)

Para uma instalação reproduzível da versão 1.0.0:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 1.0.0
```

A identidade do pacote NuGet é separada do assembly e do namespace:

```text
Pacote NuGet: TypedParameters.Dapper.SqlServer
Assembly: Dapper.TypedParameters.SqlServer.dll
Namespace: Dapper.TypedParameters.SqlServer
```

## Exemplo Mínimo

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

await using var connection = new SqlConnection(connectionString);

var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT Id, Document, Name
    FROM dbo.Customers
    WHERE Document = @Document;
    """,
    new
    {
        Document = SqlParam.VarChar(document, 11)
    });
```

```text
.NET string
  -> metadados SQL explícitos
  -> parâmetro SQL Server varchar(11)
```

## Por Quê?

A inferência de parâmetros do Dapper é correta e conveniente em muitos cenários.
O trade-off é que os metadados SQL Server enviados ao provider nem sempre ficam
óbvios no código chamador.

Quando o código já conhece o contrato do banco, metadados explícitos deixam esse
contrato visível:

```csharp
new
{
    Document = document
}
```

```csharp
new
{
    Document = SqlParam.VarChar(document, 11)
}
```

A segunda forma não afirma que `varchar` é universalmente melhor que `nvarchar`.
Ela diz que este parâmetro deve corresponder a um contrato conhecido
`varchar(11)`.

## O Problema

SQL Server avalia parâmetros usando metadados de tipos SQL, não apenas valores
CLR. Uma divergência de metadados pode causar conversões no SQL Server dependendo
dos tipos envolvidos, da precedência de tipos, da collation, do formato da query,
dos índices e do plano de execução.

Esta biblioteca dá ao chamador controle sobre os metadados enviados por
`Microsoft.Data.SqlClient`. Ela não garante queries mais rápidas, não remove
toda conversão implícita e não analisa planos de execução. Meça queries
sensíveis a performance no seu próprio workload.

## Tipos de Parâmetros Suportados

| Família | Tipos SQL Server |
| --- | --- |
| Strings | `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)`, `nvarchar(max)` |
| Numéricos | `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money`, `smallmoney` |
| Binários e identificadores | `uniqueidentifier`, `binary`, `varbinary`, `varbinary(max)` |
| Temporais | `date`, `time`, `datetime`, `smalldatetime`, `datetime2`, `datetimeoffset` |
| Parâmetros de saída | `AsOutput()`, `AsInputOutput()`, `OutputValue`, `GetValue<T>()` |
| Table-valued parameters | `SqlDbType.Structured` com `TypeName` explícito e `DataTable` fornecido pelo chamador |

## Compatibilidade

| Item | Suporte |
| --- | --- |
| Target frameworks | `net8.0`; `net10.0` |
| Dapper | `2.1.79` |
| Microsoft.Data.SqlClient | `6.1.6` |
| Npgsql | `10.0.3` |
| Providers ADO.NET | `Microsoft.Data.SqlClient` para SQL Server; `Npgsql` para PostgreSQL |
| System.Data.SqlClient | Não suportado pelo provider SQL Server |
| Compatibilidade declarada do driver SQL Server | SQL Server 2016 até SQL Server 2025 |
| SQL Server testado na CI | `mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04` |
| Azure SQL Database | Compatível pelo driver; não testado por integração neste repositório |
| Azure SQL Managed Instance | Compatível pelo driver; não testado por integração neste repositório |
| Azure Synapse Analytics | Compatível pelo driver; não testado por integração neste repositório |

As entradas de SQL Server e Azure SQL descrevem compatibilidade do driver
`Microsoft.Data.SqlClient`. Este repositório atualmente executa testes de
integração apenas na imagem SQL Server 2022 listada acima.

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

## Princípios de Design

- Tornar metadados de parâmetros provider-specific explícitos no ponto de chamada.
- Manter a API pública pequena e previsível.
- Usar os tipos ADO.NET do provider diretamente.
- Preservar os padrões normais de chamada do Dapper.
- Preferir factory methods explícitos em vez de seleção automática de tipo SQL.
- Evitar abstrações cross-provider até que responsabilidades idênticas sejam comprovadas.

## O Que Esta Biblioteca Não Faz

A biblioteca não:

- inspeciona o schema do banco;
- consulta o banco para obter metadados;
- reescreve SQL;
- analisa planos de execução;
- detecta `CONVERT_IMPLICIT`;
- escolhe tipos SQL automaticamente;
- mapeia POCOs para table-valued parameters;
- cria user-defined table types no SQL Server;
- oferece suporte a `System.Data.SqlClient`.

## Testes e Qualidade

O repositório valida testes unitários, testes de integração por provider,
conteúdo do pacote, consumo do pacote, baselines de API pública, SourceLink,
package validation e Quality Gate do SonarQube Cloud para os target frameworks
suportados.

Validação local básica:

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
```

Os testes de integração SQL Server usam Docker e `Testcontainers.MsSql`. Os
testes de integração PostgreSQL usarão `Testcontainers.PostgreSql` quando a API
funcional for implementada.

## Registros de Release

O workflow protegido de release publica o mesmo `.nupkg` validado no
NuGet.org, fonte pública principal de instalação, e no GitHub Packages, registro
secundário vinculado ao repositório. Uma simulação com `publish=false` nunca
publica; `publish=true` exige a tag correspondente à versão e a aprovação do
environment `nuget-release`. O NuGet.org usa Trusted Publishing, enquanto o
GitHub Packages usa o `GITHUB_TOKEN` efêmero do workflow. Após a primeira
publicação, o pacote no GitHub deve ser tornado público explicitamente antes de
ser consumido sem autenticação.

## Contribuindo

Issues e pull requests são bem-vindos. Mantenha mudanças pequenas, explícitas e
validadas para os dois target frameworks suportados.

## Licença

Este projeto é licenciado sob a licença MIT.

## Aviso

Este projeto não é afiliado, mantido ou endossado oficialmente pelo projeto
Dapper ou pela Microsoft.
