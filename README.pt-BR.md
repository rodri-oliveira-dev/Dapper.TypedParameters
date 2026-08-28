# Dapper.TypedParameters

[English](README.md) | Português (Brasil)

[![NuGet](https://img.shields.io/nuget/v/TypedParameters.Dapper.SqlServer?logo=nuget&label=NuGet)](https://www.nuget.org/packages/TypedParameters.Dapper.SqlServer)
[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=rodri-oliveira-dev_Dapper.TypedParameters&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rodri-oliveira-dev_Dapper.TypedParameters)
[![CI](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/rodri-oliveira-dev/Dapper.TypedParameters)

`Dapper.TypedParameters` hospeda pacotes específicos por provider para
metadados explícitos de parâmetros de banco de dados no Dapper.

```text
Dapper.TypedParameters
├── TypedParameters.Dapper.SqlServer
└── TypedParameters.Dapper.PostgreSql
```

Os pacotes são independentes. Use o pacote SQL Server com
`Microsoft.Data.SqlClient` e `SqlDbType`; use o pacote PostgreSQL com `Npgsql` e
`NpgsqlDbType`.

O repositório não expõe uma base compartilhada como `TypedDbParameter`. SQL
Server e PostgreSQL têm APIs de provider e semânticas de banco diferentes, então
cada pacote mantém seu próprio contrato público pequeno.

## Instalação

Instale o provider SQL Server:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

Instale o provider PostgreSQL:

```bash
dotnet add package TypedParameters.Dapper.PostgreSql
```

As identidades dos pacotes NuGet são separadas das identidades de assembly e
namespace:

| Pacote | Assembly | Namespace |
| --- | --- | --- |
| `TypedParameters.Dapper.SqlServer` | `Dapper.TypedParameters.SqlServer.dll` | `Dapper.TypedParameters.SqlServer` |
| `TypedParameters.Dapper.PostgreSql` | `Dapper.TypedParameters.PostgreSql.dll` | `Dapper.TypedParameters.PostgreSql` |

## Exemplo SQL Server

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

## Exemplo PostgreSQL

```csharp
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;

await using var connection = new NpgsqlConnection(connectionString);

var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT id, document, payload, created_at
    FROM customers
    WHERE document = @Document
      AND created_at >= @CreatedAt;
    """,
    new
    {
        Document = PostgresParam.VarChar(document),
        CreatedAt = PostgresParam.TimestampTz(fromUtc)
    });
```

## Por Quê?

A inferência de parâmetros do Dapper é correta e conveniente em muitos cenários.
O trade-off é que os metadados enviados ao provider nem sempre ficam óbvios no
código chamador.

Quando o código já conhece o contrato do banco, metadados explícitos deixam esse
contrato visível e testável:

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

```csharp
new
{
    Payload = PostgresParam.Jsonb(json),
    CreatedAt = PostgresParam.TimestampTz(createdAtUtc)
}
```

A biblioteca não afirma que um tipo de banco é universalmente mais rápido que
outro. Ela torna um contrato conhecido explícito; workloads sensíveis a
performance ainda precisam ser medidos.

## Tipos Suportados

SQL Server:

| Família | Tipos SQL Server |
| --- | --- |
| Strings | `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)`, `nvarchar(max)` |
| Numéricos | `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money`, `smallmoney` |
| Binários e identificadores | `uniqueidentifier`, `binary`, `varbinary`, `varbinary(max)` |
| Temporais | `date`, `time`, `datetime`, `smalldatetime`, `datetime2`, `datetimeoffset` |
| Parâmetros de saída | `AsOutput()`, `AsInputOutput()`, `OutputValue`, `GetValue<T>()` |
| Table-valued parameters | `SqlDbType.Structured` com `TypeName` explícito e `DataTable` fornecido pelo chamador |

PostgreSQL:

| Família | Factories | Tipos PostgreSQL |
| --- | --- | --- |
| Text | `Text`, `VarChar`, `Char` | `text`, `character varying`, `character` |
| Boolean/numeric | `Boolean`, `SmallInt`, `Integer`, `BigInt`, `Real`, `Double`, `Numeric`, `Money` | `boolean`, `smallint`, `integer`, `bigint`, `real`, `double precision`, `numeric`, `money` |
| Identifier/binary | `Uuid`, `Bytea` | `uuid`, `bytea` |
| JSON | `Json`, `Jsonb` | `json`, `jsonb` |
| Temporal | `Date`, `Time`, `Timestamp`, `TimestampTz`, `Interval` | `date`, `time without time zone`, `timestamp without time zone`, `timestamp with time zone`, `interval` |
| Arrays | `Array<T>(IList<T>? value, NpgsqlDbType elementType)` | `integer[]`, `uuid[]`, `text[]` e arrays de outros tipos escalares v1 suportados |

## Semântica PostgreSQL

`PostgresParam.Text(value)` envia `text`. `VarChar(value)` envia
`character varying`, e `Char(value)` envia `character`. A API PostgreSQL não
expõe `VarChar(value, size)` nem `Char(value, size)`: os testes de integração
mostraram que `NpgsqlParameter.Size` não faz o PostgreSQL observar um typmod
`varchar(n)` ou `char(n)`, e valores acima do tamanho são truncados pelo Npgsql
antes de chegar ao servidor.

`PostgresParam.Numeric(value)` envia `numeric` sem restrição declarada. Precisão
e escala não fazem parte da API pública nesta versão porque os testes de
integração mostraram `NpgsqlParameter.Precision` e `Scale` como metadados do
parâmetro no cliente, não como um contrato server-side comprovado de
`numeric(p, s)`.

`PostgresParam.Json(value)` mapeia para `json`.
`PostgresParam.Jsonb(value)` mapeia para `jsonb`. A versão 1 recebe JSON textual
fornecido pelo chamador; serialização automática de POCO está fora do escopo
deste pacote.

Factories temporais seguem a semântica PostgreSQL/Npgsql:

- `Date(DateOnly?)` envia `date`.
- `Time(TimeOnly?)` envia `time without time zone`.
- `Timestamp(DateTime?)` envia `timestamp without time zone`; aceita valores de
  relógio de parede com `DateTimeKind.Local` ou `DateTimeKind.Unspecified` e
  rejeita valores UTC.
- `TimestampTz(DateTime?)` envia `timestamp with time zone`; aceita somente
  `DateTimeKind.Utc` e não converte valores locais ou unspecified.
- `Interval(TimeSpan?)` envia `interval`; componentes de mês e ano de intervalos
  PostgreSQL não podem ser representados por `TimeSpan`.

`timestamptz` representa um instante. Ele não armazena o identificador da zona
de tempo.

Arrays são uma feature nativa do PostgreSQL. `PostgresParam.Array<T>(value,
elementType)` exige um `NpgsqlDbType` escalar explícito para o elemento e envia
`NpgsqlDbType.Array | elementType`. `null` é enviado como `DBNull.Value` com o
tipo de array declarado; arrays vazios permanecem arrays vazios. Isso não é um
equivalente de TVP do SQL Server.

## Diferenças Entre Providers

| Capacidade | Provider SQL Server | Provider PostgreSQL |
| --- | --- | --- |
| Provider ADO.NET | `Microsoft.Data.SqlClient` | `Npgsql` |
| Metadado de tipo | `SqlDbType` | `NpgsqlDbType` |
| Parâmetros de entrada | Sim | Sim |
| Helpers output/input-output | Sim | Não nesta versão |
| Parâmetros estruturados em lote | TVPs SQL Server via `DataTable` | Sem TVP artificial; use padrões nativos PostgreSQL fora deste pacote |
| Arrays | Arrays binários como valores escalares | Arrays nativos PostgreSQL com tipo de elemento explícito |
| JSON | Não modelado pelo pacote SQL Server | `json` e `jsonb` |
| Semântica temporal | Tipos temporais SQL Server | Regras PostgreSQL de `timestamp`/`timestamptz` |

## Fora de Escopo PostgreSQL

O pacote PostgreSQL não oferece suporte a estes recursos nesta versão:

- PostgreSQL enums;
- composites;
- APIs UDT genéricas com `DataTypeName`;
- ranges;
- multiranges;
- PostGIS;
- network types;
- `hstore`;
- full-text-search types;
- tipos específicos de extensões;
- NodaTime;
- serialização automática de POCO para JSON;
- `COPY` ou APIs bulk;
- inspeção de schema;
- reescrita de positional placeholders;
- paridade com output parameters estilo SQL Server.

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
| PostgreSQL testado na CI | `postgres:17.6-bookworm` |
| Azure SQL Database | Compatível pelo driver; não testado por integração neste repositório |
| Azure SQL Managed Instance | Compatível pelo driver; não testado por integração neste repositório |
| Azure Synapse Analytics | Compatível pelo driver; não testado por integração neste repositório |

## Documentação

- [Primeiros passos com SQL Server](docs/getting-started.pt-BR.md)
- [Guia do provider PostgreSQL](docs/postgresql.pt-BR.md)
- [Motivação](docs/motivation.pt-BR.md)
- Exemplos SQL Server:
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
- detecta conversões implícitas;
- escolhe tipos SQL automaticamente;
- mapeia POCOs para table-valued parameters SQL Server;
- cria user-defined table types no SQL Server;
- serializa POCOs para JSON PostgreSQL;
- emula TVPs SQL Server em PostgreSQL;
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
dotnet pack src/Dapper.TypedParameters.PostgreSql/Dapper.TypedParameters.PostgreSql.csproj --configuration Release --no-build --output artifacts/packages
```

Os testes de integração SQL Server usam Docker e `Testcontainers.MsSql`.
Os testes de integração PostgreSQL usam Docker e `Testcontainers.PostgreSql`.

## Registros de Release

O workflow protegido de release aceita uma versão SemVer `version` sem prefixo
`v`. Ele valida, empacota e publica `TypedParameters.Dapper.SqlServer` e
`TypedParameters.Dapper.PostgreSql` como pacotes NuGet separados. O NuGet.org
usa Trusted Publishing pelo environment `nuget-release`; GitHub Packages usa o
`GITHUB_TOKEN` efêmero do workflow.

## Contribuindo

Issues e pull requests são bem-vindos. Mantenha mudanças pequenas, explícitas e
validadas para os dois target frameworks suportados.

## Licença

Este projeto é licenciado sob a licença MIT.

## Aviso

Este projeto não é afiliado, mantido ou endossado oficialmente pelo projeto
Dapper, pela Microsoft, pelo PostgreSQL ou pelo projeto Npgsql.
