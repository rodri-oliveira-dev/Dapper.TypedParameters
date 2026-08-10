# 010 - Table-valued parameters

## Context

Prompt 010 adds explicit SQL Server table-valued parameter support for Dapper
using `Microsoft.Data.SqlClient`.

The feature targets existing user-defined table types in the database. The
library must materialize a parameter with `SqlDbType.Structured`, `TypeName`,
the caller-supplied value, and `ParameterDirection.Input`.

The current scalar model in `TypedSqlParameter` is intentionally focused on SQL
Server scalar metadata: `Size`, `Precision`, `Scale`, and output direction.
TVPs have a different contract and should not be forced into that shape.

## Alternatives considered

### `DataTable`

`DataTable` is available in the supported TFMs and is supported by
`Microsoft.Data.SqlClient` for TVP values. It lets the caller define columns
explicitly and add zero or more rows without introducing another public
dependency or a custom row mapper.

This option is accepted for prompt 010 because it provides a small public API:

```csharp
SqlParam.TableValued(string typeName, DataTable value);
```

The factory validates `typeName` and `value`, then returns a dedicated
`SqlMapper.ICustomQueryParameter` implementation.

### `IEnumerable<SqlDataRecord>`

`Microsoft.Data.SqlClient` 7.0.2 exposes
`Microsoft.Data.SqlClient.Server.SqlDataRecord` and related metadata types in
the provider package. They are present in the provider reference assemblies
available to `net8.0`, and `net10.0` can consume the same package surface.

This option is useful for streaming-style TVP construction and for avoiding
`DataTable` allocation. It is not added in prompt 010 because it expands the
public API with provider-specific server metadata types and would require
parallel unit and integration coverage for both target frameworks. The
`DataTable` API is enough to satisfy the current explicit TVP support.

### Automatic POCO mapping

Automatic mapping from POCO collections would require reflection, convention
decisions, nullable handling, SQL type inference, column ordering rules, and
schema validation policy. This conflicts with the package principle of explicit
SQL Server types.

This option is rejected for prompt 010.

## Decision

Add a dedicated TVP parameter type instead of extending `TypedSqlParameter`.

Accepted API:

```csharp
SqlParam.TableValued(string typeName, DataTable value);
```

The returned object implements `SqlMapper.ICustomQueryParameter` and exposes:

- `TypeName`
- `Value`
- `SqlDbType`, fixed to `SqlDbType.Structured`
- `Direction`, fixed to `ParameterDirection.Input`

It does not expose `Size`, `Precision`, `Scale`, `AsOutput()`, or
`AsInputOutput()`.

## Compatibility

The API is additive and does not change existing scalar factories.

`DataTable`, `SqlDbType.Structured`, `SqlParameter.TypeName`, and
`ParameterDirection.Input` are available for the package's supported
`net8.0` and `net10.0` targets. The implementation avoids TFM-specific code.

## `TypeName`

`typeName` is required because SQL Server needs the user-defined table type
name when binding a TVP. The library does not infer it from the `DataTable` name
or from database metadata.

Validation:

- `null` throws `ArgumentNullException`;
- empty string throws `ArgumentException`;
- whitespace-only string throws `ArgumentException`.

The value is preserved as supplied so callers can choose names such as
`dbo.CustomerBatch` or `[dbo].[CustomerBatch]`.

## Empty Collections

An empty `DataTable` is supported. The caller must still define the columns in
the expected order and with compatible CLR types. SQL Server receives a TVP with
zero rows.

## Null Values

The `DataTable` instance cannot be `null`; `SqlParam.TableValued` throws
`ArgumentNullException` for a null table.

Rows inside the `DataTable` may use `DBNull.Value` according to normal
`DataTable` and SQL Server rules. A null TVP is not modeled by this prompt; use
an empty table when the intended value is an empty set.

## Schema Validation

The library does not query the database, introspect the table type, or compare
`DataTable` columns against the SQL Server user-defined table type.

SQL Server and `Microsoft.Data.SqlClient` perform binding and schema validation
when the command executes. Mismatches surface as provider or SQL Server errors.

## Responsibilities

The library is responsible for:

- rejecting invalid `typeName` and null `DataTable`;
- requiring a `Microsoft.Data.SqlClient.SqlCommand`;
- creating or reusing a `SqlParameter`;
- setting `SqlDbType.Structured`;
- setting `TypeName`;
- setting `Value`;
- setting `ParameterDirection.Input`;
- avoiding unsupported TVP metadata such as `Size`, `Precision`, `Scale`, and
  output directions.

The caller is responsible for:

- creating the user-defined table type in SQL Server;
- building a `DataTable` whose columns match that table type;
- choosing the correct `TypeName`;
- handling provider or SQL Server schema errors.

## Tests

Unit tests must cover:

- factory type and `ICustomQueryParameter` implementation;
- `TypeName`;
- `SqlDbType.Structured`;
- filled `DataTable`;
- empty `DataTable`;
- null `DataTable`;
- null, empty, and whitespace `typeName`;
- materialized `SqlParameter` metadata;
- incompatible provider command;
- absence of scalar metadata and output APIs.

Integration tests must create exclusive SQL Server table types and auxiliary
objects, then clean them up. They cover:

- TVP with one row;
- TVP with multiple rows;
- empty TVP;
- multiple columns;
- different column types;
- stored procedure usage;
- command SQL usage;
- insertion into a table;
- aggregate result;
- asynchronous Dapper execution;
- validation through both `net8.0` and `net10.0` test targets.

## Risks

The main risk is callers assuming the library validates schema compatibility
before execution. It does not, by design, because that would require database
queries and permissions outside the package scope.

Another risk is expanding the API too early with POCO mapping or
`SqlDataRecord` overloads. Those remain future decisions so the prompt 010
surface stays small and explicit.
