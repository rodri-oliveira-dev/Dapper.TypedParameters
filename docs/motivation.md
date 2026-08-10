# Motivation

[Back to README](../README.md) | [Getting started](getting-started.md)

`Dapper.TypedParameters.SqlServer` exists because the type of a SQL Server
parameter is part of the contract between application code and the database.
Dapper keeps the calling model small, while ADO.NET providers still need to turn
CLR values into provider parameters.

## Parameter inference

When a Dapper call receives an anonymous object, Dapper and the provider build
the database parameters that will be sent with the command. A value such as a
.NET `string`, `decimal`, `DateTime`, `Guid`, or `byte[]` needs SQL Server
metadata before SQL Server can execute the query.

Inference is convenient and often exactly what an application needs. The cost is
that the inferred SQL metadata is not explicit in the calling code.

## SQL type metadata

The library exposes the metadata that is applicable to the current API:

- `SqlDbType`: the SQL Server type, such as `VarChar`, `Decimal`, or
  `Structured`.
- `Size`: bounded string and binary length, with `-1` representing SQL Server
  `max` types.
- `Precision`: declared only for `decimal`.
- `Scale`: declared for `decimal`, `time`, `datetime2`, and `datetimeoffset`.
- `Direction`: `Input`, `Output`, or `InputOutput` for scalar parameters.
- `TypeName`: user-defined table type name for table-valued parameters.

## Type mismatch

A type mismatch happens when the parameter sent by the client is not the same
SQL Server type expected by the schema or stored procedure contract. For
example, code may send a string parameter while the column being compared is
`varchar(11)`, or it may omit the intended precision and scale for
`decimal(18, 2)`.

This library makes the database type part of the calling code's intent:

```csharp
Amount = SqlParam.Decimal(amount, precision: 18, scale: 2)
```

## Implicit conversion

SQL Server can apply implicit conversions when comparing or assigning values of
different SQL types. Whether that matters depends on the exact types, type
precedence, collation, predicates, indexes, and execution plan.

The presence of a mismatch does not mean a query is automatically slow, and a
typed parameter does not guarantee a better plan. Explicit metadata simply gives
the caller a way to send the SQL type they intended.

## Query plans

Parameter type differences may influence how SQL Server evaluates expressions.
In some workloads, avoiding an unintended mismatch can help preserve the plan
shape the schema was designed for. In other workloads, there may be no measurable
difference.

Performance-sensitive code should be verified with representative data,
statistics, indexes, and execution plans.

## Explicit intent

The main benefit is intent:

```text
the database type becomes part of the calling code's intent
```

The caller chooses `SqlParam.VarChar`, `SqlParam.NVarChar`,
`SqlParam.Decimal`, `SqlParam.DateTime2`, or another factory because the database
contract is known at that call site.

## Trade-offs

- More schema knowledge appears in application code.
- The caller can declare the wrong SQL type.
- Schema changes may require code changes.
- Calls are more verbose than ordinary anonymous object values.
- The library does not introspect schema automatically.
