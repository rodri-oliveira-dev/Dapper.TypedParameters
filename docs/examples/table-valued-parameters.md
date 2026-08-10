# Table-Valued Parameters

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Table-valued parameters use an existing SQL Server user-defined table type and a
caller-supplied `DataTable`.

## Create the table type

```sql
CREATE TYPE dbo.CustomerBatch AS TABLE
(
    CustomerId int NOT NULL,
    Name nvarchar(100) NOT NULL,
    IsActive bit NOT NULL
);
```

The library does not create this type.

## Build the DataTable

```csharp
using System.Data;

using var customers = new DataTable();
customers.Columns.Add("CustomerId", typeof(int));
customers.Columns.Add("Name", typeof(string));
customers.Columns.Add("IsActive", typeof(bool));
customers.Rows.Add(1, "Ada Lovelace", true);
customers.Rows.Add(2, "Grace Hopper", true);
```

## Call Dapper

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

`SqlParam.TableValued(typeName, value)` materializes `SqlDbType.Structured`,
`TypeName`, the supplied `DataTable`, and `ParameterDirection.Input`.

## Empty tables

An empty `DataTable` is supported when its columns are declared:

```csharp
using var customers = new DataTable();
customers.Columns.Add("CustomerId", typeof(int));
customers.Columns.Add("Name", typeof(string));
customers.Columns.Add("IsActive", typeof(bool));

var parameter = SqlParam.TableValued("dbo.CustomerBatch", customers);
```

Use an empty table when the intended value is an empty set. A null `DataTable`
is rejected.

## Schema responsibility

The caller is responsible for:

- creating the user-defined table type;
- choosing the correct `TypeName`;
- building a `DataTable` whose columns match the table type;
- handling provider or SQL Server errors for schema mismatches.

The library does not discover table type schema, map POCOs, query SQL Server, or
validate columns automatically.
