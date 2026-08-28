# Primeiros Passos

[English](getting-started.md) | Português (Brasil)

[Voltar ao README](../README.pt-BR.md) | [Motivação](motivation.pt-BR.md)

Este guia mostra padrões básicos de Dapper com o provider SQL Server,
`Dapper.TypedParameters.SqlServer` 1.0.0. Para PostgreSQL, veja o
[guia do provider PostgreSQL](postgresql.pt-BR.md).

## Pré-requisitos

- Um projeto com `net8.0` ou `net10.0`.
- Dapper.
- `Microsoft.Data.SqlClient`.
- Um banco SQL Server.

## Instalação

Instale o pacote stable atual pelo NuGet.org:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

Para uma instalação reproduzível da versão 1.0.0:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 1.0.0
```

Use o namespace do pacote no C#:

```csharp
using Dapper.TypedParameters.SqlServer;
```

## Imports

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;
```

## Criar uma Conexão

```csharp
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
```

## Primeiro SELECT

```csharp
var customer = await connection.QuerySingleOrDefaultAsync<Customer>(
    """
    SELECT Id, Document, Name
    FROM dbo.Customers
    WHERE Document = @Document;
    """,
    new
    {
        Document = SqlParam.VarChar("12345678901", 11)
    });
```

## INSERT

```csharp
int rows = await connection.ExecuteAsync(
    """
    INSERT INTO dbo.Customers (Document, Name)
    VALUES (@Document, @Name);
    """,
    new
    {
        Document = SqlParam.VarChar("12345678901", 11),
        Name = SqlParam.NVarChar("Ada Lovelace", 100)
    });
```

## UPDATE

```csharp
int rows = await connection.ExecuteAsync(
    """
    UPDATE dbo.Customers
    SET Name = @Name
    WHERE Document = @Document;
    """,
    new
    {
        Document = SqlParam.VarChar("12345678901", 11),
        Name = SqlParam.NVarChar("Grace Hopper", 100)
    });
```

## DELETE

```csharp
int rows = await connection.ExecuteAsync(
    "DELETE FROM dbo.Customers WHERE Document = @Document;",
    new
    {
        Document = SqlParam.VarChar("12345678901", 11)
    });
```

## Valores Nulos

```csharp
await connection.ExecuteAsync(
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
```

`null` é convertido para `DBNull.Value` quando o parâmetro é materializado para
`Microsoft.Data.SqlClient`.

## Múltiplos Parâmetros

```csharp
var invoice = await connection.QuerySingleAsync<Invoice>(
    """
    SELECT Id, CustomerId, Amount
    FROM dbo.Invoices
    WHERE CustomerId = @CustomerId
      AND Amount >= @MinimumAmount;
    """,
    new
    {
        CustomerId = SqlParam.Int(42),
        MinimumAmount = SqlParam.Decimal(100M, precision: 18, scale: 2)
    });
```

## Exemplos Especializados

- [Guia do provider PostgreSQL](postgresql.pt-BR.md)
- [Strings](examples/strings.pt-BR.md)
- [Numéricos](examples/numeric.pt-BR.md)
- [Binários e identificadores](examples/binary.pt-BR.md)
- [Temporais](examples/temporal.pt-BR.md)
- [Parâmetros de saída](examples/output-parameters.pt-BR.md)
- [Table-valued parameters](examples/table-valued-parameters.pt-BR.md)
