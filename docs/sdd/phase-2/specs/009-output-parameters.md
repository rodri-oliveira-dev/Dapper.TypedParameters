# 009 - Output parameters

## Context

Prompt 009 adds `Output` and `InputOutput` support for scalar
`TypedSqlParameter` instances used by Dapper with `Microsoft.Data.SqlClient`.

Structured parameters are outside this prompt. The output behavior applies to
all scalar factories available at this point:

- string parameters;
- numeric and boolean parameters;
- `uniqueidentifier`;
- binary parameters;
- temporal parameters.

The accessible remote `main` did not include the expected previous phase-2
state. This branch combines the local prompt 006/007 branch with prompt 008
before defining output support so the API can cover every scalar family present
in the workspace.

## Alternatives considered

### Separate factories: `SqlParam.Output.*`

Separate output factory families make direction visually explicit and avoid
mutating an existing instance. They also double the public surface and require
mirroring every existing and future scalar factory under a second namespace.
That increases documentation, testing, discoverability, and compatibility cost.

This option was rejected because the ergonomics are worse for a small library
whose existing value is explicit but compact factory naming.

### Optional `direction` parameter on factories

Adding `ParameterDirection direction = ParameterDirection.Input` to every
factory keeps object shape simple, but it changes every signature and introduces
optional arguments next to existing optional `scale` parameters. That raises
ambiguity and call-site readability risk, especially for temporal factories.

This option was rejected because the prompt explicitly disallows optional
parameters that cause ambiguity and because future factories would inherit the
same signature pressure.

### Fluent methods: `AsOutput()` and `AsInputOutput()`

Fluent methods preserve all current factories and keep `Input` as the default.
They compose naturally with existing calls:

```csharp
var result = SqlParam.VarChar(null, 100).AsOutput();
var counter = SqlParam.Int(1).AsInputOutput();
```

The resulting instance retains the same metadata and owns the materialized
`SqlParameter` reference created by Dapper. This keeps value retrieval attached
to the instance the caller already has to retain.

This option is accepted.

### Specific output class

A dedicated output class could make lifecycle state more explicit, but it would
split the public model and make fluent conversion or covariance necessary.
Because Dapper consumes `SqlMapper.ICustomQueryParameter`, a second public class
does not materially improve safety unless the factory surface also expands.

This option was rejected as unnecessary API weight.

## Decision

Add fluent methods to `TypedSqlParameter`:

- `AsOutput()`
- `AsInputOutput()`

Both methods return a new `TypedSqlParameter` with the same SQL metadata and the
requested `ParameterDirection`. Existing factory methods continue to create
`Input` parameters.

Expose value retrieval through:

- `object? OutputValue`
- `T? GetValue<T>()`

Do not expose the mutable `SqlParameter` publicly.

## Compatibility

No existing factory signatures change, and `Input` remains the default. Existing
call sites continue to compile and behave as before.

The new fluent methods are additive. They avoid optional direction arguments and
avoid introducing parallel factory names.

## Retrieval and lifecycle

`TypedSqlParameter` retains the `SqlParameter` instance materialized by
`AddParameter`. After Dapper executes the command, reading `OutputValue` or
`GetValue<T>()` reads the retained parameter's current `Value`.

If the value is requested before materialization, the API throws an
`InvalidOperationException` explaining that the parameter has not been
materialized by Dapper yet.

If the retained parameter still contains `DBNull.Value`, `OutputValue` returns
`null`. `GetValue<T>()` returns `null` for reference types and nullable value
types. For non-nullable value types, `GetValue<T>()` throws an
`InvalidOperationException` rather than silently returning `default`.

The library cannot perfectly prove command execution happened, because Dapper
and ADO.NET expose only the materialization hook. The documented lifecycle is
therefore: create the parameter, pass the same instance to Dapper, await command
completion, then read the output value. Reading immediately after materialization
but before command execution is considered caller misuse; tests cover the clear
pre-materialization failure.

## Type conversion

`GetValue<T>()` permits:

- exact runtime type matches;
- nullable wrappers around the exact runtime type;
- reference/interface/base-type assignments that normal CLR casting supports.

It does not perform `Convert.ChangeType`, parsing, narrowing, widening, or other
silent conversions. Incompatible requested types throw `InvalidCastException`
with the actual and requested types in the message.

## Concurrency, reuse, and thread safety

An output-capable `TypedSqlParameter` owns mutable lifecycle state: the retained
`SqlParameter` from the latest materialization. The same instance must not be
used concurrently in multiple commands.

Non-concurrent reuse is allowed. A later command materialization replaces the
retained parameter reference, and subsequent reads return the latest command's
output value.

The type is not thread-safe for concurrent materialization or output reads while
another command is executing. This matches the mutable `SqlParameter` lifecycle
and will be documented.

## `DBNull.Value`

Input `null` continues to materialize as `DBNull.Value`. Output `DBNull.Value`
is normalized to `null` for `OutputValue`.

`GetValue<T>()` returns `null` only when `T` can represent null. For
non-nullable value types it throws a clear `InvalidOperationException`.

## Tests

Unit tests must cover:

- `Input` as the default direction;
- `Output` and `InputOutput` direction configuration;
- materialized `SqlParameter.Direction`;
- early read before materialization;
- `DBNull.Value`;
- valid typed retrieval;
- invalid typed retrieval;
- non-concurrent reuse;
- materialization retaining the parameter;
- every scalar factory family.

Integration tests must use Dapper against SQL Server stored procedures for:

- output `varchar`;
- output `nvarchar`;
- output `int`;
- output `decimal`;
- output `datetime2`;
- output `uniqueidentifier`;
- output null;
- input/output;
- multiple outputs;
- asynchronous execution;
- both `net8.0` and `net10.0`.

Created stored procedures must be exclusive to the tests and removed by the
tests.
