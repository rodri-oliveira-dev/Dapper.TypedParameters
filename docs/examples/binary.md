# Binary and Identifier Parameters

English | [Português (Brasil)](binary.pt-BR.md)

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Binary and identifier factories cover `uniqueidentifier`, `binary`,
`varbinary`, and `varbinary(max)`.

| Factory | SQL Server type | Size |
| --- | --- | --- |
| `SqlParam.UniqueIdentifier(value)` | `uniqueidentifier` | none |
| `SqlParam.Binary(value, size)` | `binary(size)` | 1 to 8,000 |
| `SqlParam.VarBinary(value, size)` | `varbinary(size)` | 1 to 8,000 |
| `SqlParam.VarBinaryMax(value)` | `varbinary(max)` | `Size = -1` |

## uniqueidentifier

```csharp
var file = await connection.QuerySingleOrDefaultAsync<FileRow>(
    "SELECT Id, Name FROM dbo.Files WHERE Id = @Id;",
    new
    {
        Id = SqlParam.UniqueIdentifier(fileId)
    });
```

`Guid.Empty` is accepted. `null` is converted to `DBNull.Value` when the
parameter is materialized.

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

## Empty Arrays and Null

```csharp
EmptyPayload = SqlParam.VarBinary(Array.Empty<byte>(), 1)
MissingPayload = SqlParam.VarBinary(null, 1)
```

Empty arrays remain empty arrays. Only `null` is converted to `DBNull.Value`.

The library stores the supplied `byte[]` reference. It does not copy arrays,
truncate content, pad content, or validate `value.Length <= size`.
