# Table-Valued Parameters

[English](table-valued-parameters.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Table-valued parameters usam um user-defined table type SQL Server existente e
um `DataTable` fornecido pelo chamador.

## Criar o table type

```sql
CREATE TYPE dbo.CustomerBatch AS TABLE
(
    CustomerId int NOT NULL,
    Name nvarchar(100) NOT NULL,
    IsActive bit NOT NULL
);
```

A biblioteca não cria esse tipo.

## Montar o DataTable

```csharp
using System.Data;

using var customers = new DataTable();
customers.Columns.Add("CustomerId", typeof(int));
customers.Columns.Add("Name", typeof(string));
customers.Columns.Add("IsActive", typeof(bool));
customers.Rows.Add(1, "Ada Lovelace", true);
customers.Rows.Add(2, "Grace Hopper", true);
```

## Chamar Dapper

```csharp
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

int rows = await connection.ExecuteAsync(
    """
    INSERT INTO dbo.Customers (CustomerId, Name, IsActive)
    SELECT CustomerId, Name, IsActive
    FROM @Customers;
    """,
    new
    {
        Customers = SqlParam.TableValued("dbo.CustomerBatch", customers)
    });
```

`SqlParam.TableValued(typeName, value)` materializa `SqlDbType.Structured`,
`TypeName`, o `DataTable` fornecido e `ParameterDirection.Input`.

## Tabelas vazias

Um `DataTable` vazio é suportado quando suas colunas estão declaradas:

```csharp
using var customers = new DataTable();
customers.Columns.Add("CustomerId", typeof(int));
customers.Columns.Add("Name", typeof(string));
customers.Columns.Add("IsActive", typeof(bool));

var parameter = SqlParam.TableValued("dbo.CustomerBatch", customers);
```

Use uma tabela vazia quando a intenção for enviar um conjunto vazio. Um
`DataTable` nulo é rejeitado.

## Responsabilidade pelo schema

O chamador é responsável por:

- criar o user-defined table type;
- escolher o `TypeName` correto;
- montar um `DataTable` cujas colunas correspondam ao table type;
- tratar erros do provider ou do SQL Server para divergências de schema.

A biblioteca não descobre o schema do table type, não mapeia POCOs, não consulta
SQL Server e não valida colunas automaticamente.
