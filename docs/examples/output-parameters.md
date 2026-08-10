# Output Parameters

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Scalar parameters can be configured as `Output` or `InputOutput` with fluent
methods. Table-valued parameters are input-only in this API.

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

Keep the same parameter instance that you pass to Dapper. Read `OutputValue` or
`GetValue<T>()` only after `Execute` or `ExecuteAsync` completes.

## InputOutput

```csharp
var counter = SqlParam.Int(41).AsInputOutput();

await connection.ExecuteAsync(
    "dbo.IncrementCounter",
    new { Counter = counter },
    commandType: CommandType.StoredProcedure);

int next = counter.GetValue<int>();
```

## DBNull.Value

`OutputValue` normalizes `DBNull.Value` to `null`.

```csharp
string? value = output.GetValue<string?>();
```

For non-nullable value types, `GetValue<T>()` throws if the database value is
null. It does not return `default` silently.

`GetValue<T>()` uses normal CLR casting rules. It does not parse strings or call
`Convert.ChangeType`.

## Concurrency and reuse

Output parameter instances retain the latest materialized `SqlParameter`
internally. Reuse is supported for non-concurrent commands, but the same output
instance must not be shared concurrently across commands.
