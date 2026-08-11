# Output Parameters

English | [Português (Brasil)](output-parameters.pt-BR.md)

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Scalar parameters can be configured as `Output` or `InputOutput` with fluent
methods. Table-valued parameters are input-only in this API.

## Lifecycle

```text
create parameter
  -> pass the same instance to Dapper
  -> execute the command
  -> read OutputValue or GetValue<T>()
```

Read output values only after Dapper has materialized the parameter and command
execution has completed. Reading before materialization throws
`InvalidOperationException`.

## Output

Stored procedure:

```sql
CREATE PROCEDURE dbo.CreateCustomer
    @Name nvarchar(100),
    @CustomerId int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @CustomerId = 42;
END
```

Dapper call:

```csharp
using System.Data;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var customerId = SqlParam.Int(null).AsOutput();

await connection.ExecuteAsync(
    "dbo.CreateCustomer",
    new
    {
        Name = SqlParam.NVarChar("Ada Lovelace", 100),
        CustomerId = customerId
    },
    commandType: CommandType.StoredProcedure);

int id = customerId.GetValue<int>();
```

## InputOutput

```csharp
var counter = SqlParam.Int(41).AsInputOutput();

await connection.ExecuteAsync(
    "dbo.IncrementCounter",
    new { Counter = counter },
    commandType: CommandType.StoredProcedure);

int next = counter.GetValue<int>();
```

## OutputValue

```csharp
object? raw = customerId.OutputValue;
```

`OutputValue` returns the materialized provider value after execution.
`DBNull.Value` is normalized to `null`.

## GetValue<T>()

```csharp
int id = customerId.GetValue<int>();
string? optionalCode = code.GetValue<string?>();
```

`GetValue<T>()` uses normal CLR casting rules. It does not parse strings, call
`Convert.ChangeType`, or return `default` silently for database null assigned to
a non-nullable value type. Incompatible casts throw `InvalidCastException`.

## Reuse

Output parameter instances retain the latest materialized `SqlParameter`
internally. Reuse is supported for non-concurrent commands, but the same output
instance must not be shared concurrently across commands.
