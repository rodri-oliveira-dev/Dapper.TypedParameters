# Guia do Provider PostgreSQL

[English](postgresql.md) | Português (Brasil)

[Voltar ao README](../README.pt-BR.md) | [Primeiros passos SQL Server](getting-started.pt-BR.md)

Este guia cobre o pacote PostgreSQL:

```text
Pacote NuGet: TypedParameters.Dapper.PostgreSql
Assembly: Dapper.TypedParameters.PostgreSql.dll
Namespace: Dapper.TypedParameters.PostgreSql
Provider ADO.NET: Npgsql
```

O pacote existe para tornar metadados de parâmetros PostgreSQL explícitos no
ponto de chamada do Dapper. Ele envia `NpgsqlDbType` diretamente e não
inspeciona schema, não reescreve SQL e não infere tipos de banco a partir de
valores CLR.

## Primeiros Passos

Instale o pacote:

```bash
dotnet add package TypedParameters.Dapper.PostgreSql
```

Use o namespace PostgreSQL com Dapper e Npgsql:

```csharp
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;
```

```csharp
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

## Tipos Suportados

| Família | Factories | Tipos PostgreSQL |
| --- | --- | --- |
| Text | `Text`, `VarChar`, `Char` | `text`, `character varying`, `character` |
| Boolean/numeric | `Boolean`, `SmallInt`, `Integer`, `BigInt`, `Real`, `Double`, `Numeric`, `Money` | `boolean`, `smallint`, `integer`, `bigint`, `real`, `double precision`, `numeric`, `money` |
| Identifier/binary | `Uuid`, `Bytea` | `uuid`, `bytea` |
| JSON | `Json`, `Jsonb` | `json`, `jsonb` |
| Temporal | `Date`, `Time`, `Timestamp`, `TimestampTz`, `Interval` | `date`, `time without time zone`, `timestamp without time zone`, `timestamp with time zone`, `interval` |
| Arrays | `Array<T>(IList<T>? value, NpgsqlDbType elementType)` | arrays usando `NpgsqlDbType.Array | elementType` |

Todas as factories retornam `TypedPostgresParameter`, que expõe:

```csharp
public object? Value { get; }
public NpgsqlDbType NpgsqlDbType { get; }
```

Quando o Dapper materializa o parâmetro, `null` vira `DBNull.Value`,
`NpgsqlDbType` é atribuído explicitamente e a direção do parâmetro é `Input`.

## Text, Varchar, Char e Size

`PostgresParam.Text(value)` envia `NpgsqlDbType.Text`.
`PostgresParam.VarChar(value)` envia `NpgsqlDbType.Varchar`.
`PostgresParam.Char(value)` envia `NpgsqlDbType.Char`.

O provider PostgreSQL não expõe factories `VarChar` ou `Char` com tamanho. Os
testes de integração verificaram este comportamento com metadados brutos de
`NpgsqlParameter`:

| Metadado | PostgreSQL observa | Comportamento observado do Size |
| --- | --- | --- |
| `NpgsqlDbType.Varchar` | `character varying` | nenhum typmod `varchar(n)` observado |
| `NpgsqlDbType.Char` | `character` | nenhum typmod `char(n)` observado |
| `NpgsqlParameter.Size = 3` | mesmo tipo backend | valores acima do tamanho foram truncados antes de chegar ao PostgreSQL |

Isso é intencionalmente diferente do provider SQL Server, onde
`SqlParameter.Size` faz parte do contrato de parâmetro SQL Server exposto pelo
pacote.

## Numeric, Precision e Scale

`PostgresParam.Numeric(value)` envia `NpgsqlDbType.Numeric`.

Precisão e escala não fazem parte da API pública PostgreSQL nesta versão. Testes
de integração usando `NpgsqlParameter.Precision` e `Scale` mostraram o
PostgreSQL observando o tipo backend como `numeric`, enquanto um valor acima do
metadado declarado fez round-trip sem arredondamento, truncamento ou validação
server-side de `numeric(p, s)`.

O pacote, portanto, documenta `numeric` como tipo PostgreSQL explícito, não como
mecanismo de declaração de typmod.

## JSON

`PostgresParam.Json(value)` mapeia para PostgreSQL `json`.
`PostgresParam.Jsonb(value)` mapeia para PostgreSQL `jsonb`.

A versão 1 aceita JSON textual fornecido pelo chamador:

```csharp
new
{
    Payload = PostgresParam.Jsonb("{\"active\":true}")
}
```

Serialização automática de POCO, escolhas de política de `System.Text.Json`,
`JsonDocument` e mapeamento JSON global do Npgsql ficam fora do escopo v1 deste
pacote.

## Temporal

As factories temporais PostgreSQL seguem intencionalmente a semântica de
Npgsql/PostgreSQL:

| Factory | Valor CLR | Tipo PostgreSQL | Contrato |
| --- | --- | --- | --- |
| `Date` | `DateOnly?` | `date` | data de calendário |
| `Time` | `TimeOnly?` | `time without time zone` | hora de relógio de parede |
| `Timestamp` | `DateTime?` | `timestamp without time zone` | timestamp de relógio de parede; aceita `Local` e `Unspecified`, rejeita `Utc` |
| `TimestampTz` | `DateTime?` | `timestamp with time zone` | instante UTC; aceita somente `DateTimeKind.Utc` |
| `Interval` | `TimeSpan?` | `interval` | duração representável por `TimeSpan` |

`TimestampTz` não converte valores locais ou unspecified. O chamador deve passar
um `DateTime` UTC. PostgreSQL `timestamptz` representa um instante e não
armazena identificador de zona de tempo.

`Timestamp` representa `timestamp without time zone`. Ele é para valores de
relógio de parede e rejeita `DateTime` UTC para não misturar instante com
timestamp local.

`Interval` usa `TimeSpan`. Intervalos PostgreSQL podem incluir componentes de
mês e ano, que `TimeSpan` não consegue representar.

## Arrays

Arrays PostgreSQL são recursos nativos provider-specific. Eles não são modelados
como TVPs SQL Server.

```csharp
new
{
    CustomerIds = PostgresParam.Array(customerIds, NpgsqlDbType.Integer),
    ExternalIds = PostgresParam.Array(externalIds, NpgsqlDbType.Uuid),
    Tags = PostgresParam.Array(tags, NpgsqlDbType.Text)
}
```

Comportamento suportado:

- `elementType` é explícito e deve ser um `NpgsqlDbType` escalar suportado.
- `integer[]`, `uuid[]` e `text[]` são cobertos por testes de integração.
- `T[]` e `List<T>` são aceitos por `IList<T>` sem cópia.
- `null` é enviado como `DBNull.Value` com o tipo de array declarado.
- arrays vazios permanecem arrays vazios.

Limitações:

- `elementType` não pode incluir `Array`, `Range` ou `Multirange`.
- arrays de arrays não são suportados nesta versão.
- arrays cujo tipo de elemento exige `DataTypeName` ou mapeamento customizado do
  Npgsql ficam fora desta versão.

## Diferenças e Limitações

| Área | SQL Server | PostgreSQL |
| --- | --- | --- |
| Pacote provider | `TypedParameters.Dapper.SqlServer` | `TypedParameters.Dapper.PostgreSql` |
| Metadado de tipo | `SqlDbType` | `NpgsqlDbType` |
| Provider ADO.NET | `Microsoft.Data.SqlClient` | `Npgsql` |
| API output/input-output | Suportada por parâmetros escalares SQL Server | Não copiada nesta versão |
| Parâmetro em lote com formato de linhas | TVP SQL Server com `TypeName` e `DataTable` explícitos | Sem abstração artificial de TVP |
| Arrays | Sem recurso genérico de array SQL Server | Arrays nativos PostgreSQL |
| JSON | Não exposto no pacote SQL Server | `json` e `jsonb` |
| Semântica temporal | Família temporal SQL Server | Regras PostgreSQL de `timestamp` e `timestamptz` |

Não há `TypedDbParameter` compartilhado porque uma base comum esconderia
comportamentos provider-specific importantes. A duplicação entre providers é
aceita quando preserva a clareza do contrato de cada banco.

O pacote PostgreSQL não oferece suporte a:

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
