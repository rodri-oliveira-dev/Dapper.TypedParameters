# Binary and Identifier Parameters

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Binary and identifier factories cover `uniqueidentifier`, `binary`,
`varbinary`, and `varbinary(max)`.

| Factory | SQL Server type |
| --- | --- |
| `SqlParam.UniqueIdentifier(value)` | `uniqueidentifier` |
| `SqlParam.Binary(value, size)` | `binary(size)` |
| `SqlParam.VarBinary(value, size)` | `varbinary(size)` |
| `SqlParam.VarBinaryMax(value)` | `varbinary(max)` |

## uniqueidentifier

```csharp
var file = await connection.QuerySingleOrDefaultAsync<FileRow>(
    "SELECT Id, Name FROM dbo.Files WHERE Id = @Id;",
    new
    {
        Id = SqlParam.UniqueIdentifier(fileId)
    });
```

`Guid.Empty` is accepted. `null` is materialized as `DBNull.Value`.

## binary and varbinary

```csharp
await connection.ExecuteAsync(
    """
    INSERT INTO dbo.Files (Id, Checksum, Payload)
    VALUES (@Id, @Checksum, @Payload);
    """,
    new
    {
        Id = SqlParam.UniqueIdentifier(Guid.NewGuid()),
        Checksum = SqlParam.Binary(checksum, 32),
        Payload = SqlParam.VarBinary(payload, 8000)
    });
```

`Binary` and `VarBinary` accept sizes from 1 to 8,000.

## varbinary(max)

```csharp
Payload = SqlParam.VarBinaryMax(payload)
```

`VarBinaryMax` uses `SqlDbType.VarBinary` with `Size = -1`.

## Empty arrays and null

```csharp
EmptyPayload = SqlParam.VarBinary(Array.Empty<byte>(), 1)
MissingPayload = SqlParam.VarBinary(null, 1)
```

Empty arrays remain empty arrays. Only `null` is materialized as
`DBNull.Value`.

The library stores the supplied `byte[]` reference. It does not copy arrays,
truncate content, pad content, or validate `value.Length <= size`.
