# Motivation

English | [Português (Brasil)](motivation.pt-BR.md)

[Back to README](../README.md) | [Getting started](getting-started.md)

`Dapper.TypedParameters.SqlServer` exists because the type of a SQL Server
parameter is part of the contract between application code and the database.
Dapper keeps parameter passing small and convenient, while ADO.NET providers
still need to materialize CLR values as SQL Server parameters.

## CLR Values and SQL Metadata

A CLR value such as `string`, `decimal`, `DateOnly`, `TimeOnly`, `DateTime`,
`DateTimeOffset`, `Guid`, or `byte[]` is not the complete SQL Server parameter
contract by itself. The provider also sends metadata such as `SqlDbType`, size,
precision, scale, direction, and, for table-valued parameters, `TypeName`.

`SqlParameter` is the provider object that carries that metadata to
`Microsoft.Data.SqlClient`.

## Parameter Inference

Dapper and the provider can infer parameter metadata from ordinary anonymous
object values:

```csharp
new
{
    Document = "12345678901"
}
```

That inference is useful and correct for many applications. The trade-off is
that the SQL type sent to SQL Server is not explicit in the calling code.

## Explicit SQL Server Metadata

When the expected database contract is known, the caller can make it explicit:

```csharp
new
{
    Document = SqlParam.VarChar(document, 11),
    Amount = SqlParam.Decimal(amount, precision: 18, scale: 2)
}
```

The library then materializes provider parameters with the declared
`SqlDbType`, `Size`, `Precision`, and `Scale`.

## varchar and nvarchar

A common example is a .NET `string` compared with a SQL Server column declared
as `varchar(11)` or `nvarchar(150)`.

```csharp
Document = SqlParam.VarChar(document, 11)
Name = SqlParam.NVarChar(name, 150)
```

The library does not assume that `varchar` is better than `nvarchar`. The right
choice is the one that matches the schema or stored procedure contract.

## Implicit Conversions

SQL Server can apply implicit conversions when expressions combine different SQL
types. Whether a conversion appears, and whether it matters, depends on SQL
Server type precedence, collation, predicates, indexes, query shape, and the
final execution plan.

Explicit parameter metadata can help avoid unintended mismatches when the
caller knows the expected SQL Server type. It does not guarantee faster queries
or remove every possible conversion.

## Indexes and Execution Plans

Parameter metadata can influence how SQL Server evaluates predicates and whether
an index access pattern remains useful for a particular query. The outcome is
plan-dependent.

Performance-sensitive code should be measured with representative data,
statistics, indexes, and execution plans.

## Trade-offs

- More schema knowledge appears in application code.
- The caller can declare the wrong SQL type.
- Schema changes may require code changes.
- Calls are more explicit than ordinary anonymous object values.
- The library does not inspect schema automatically.
