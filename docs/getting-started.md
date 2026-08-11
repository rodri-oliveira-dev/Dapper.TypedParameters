# Getting Started

English | [Português (Brasil)](getting-started.pt-BR.md)

[Back to README](../README.md) | [Motivation](motivation.md)

This guide shows the basic Dapper patterns with
`Dapper.TypedParameters.SqlServer` 1.0.0.

## Prerequisites

- A project targeting `net8.0` or `net10.0`.
- Dapper.
- `Microsoft.Data.SqlClient`.
- A SQL Server database.

## Installation

Install the latest stable package from NuGet.org:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

For a reproducible 1.0.0 install:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 1.0.0
```

Use the package namespace in C#:

```csharp
using Dapper.TypedParameters.SqlServer;
```

## Imports

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;
```

## Create a Connection

```csharp
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
```

## First SELECT

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

## Null Values

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

`null` is converted to `DBNull.Value` when the parameter is materialized for
`Microsoft.Data.SqlClient`.

## Multiple Parameters

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

## Specialized Examples

- [Strings](examples/strings.md)
- [Numeric](examples/numeric.md)
- [Binary and identifiers](examples/binary.md)
- [Temporal](examples/temporal.md)
- [Output parameters](examples/output-parameters.md)
- [Table-valued parameters](examples/table-valued-parameters.md)
