# Parâmetros Binários e Identificadores

[English](binary.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Factories binárias e de identificadores cobrem `uniqueidentifier`, `binary`,
`varbinary` e `varbinary(max)`.

| Factory | Tipo SQL Server |
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

`Guid.Empty` é aceito. `null` é materializado como `DBNull.Value`.

## binary e varbinary

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

`Binary` e `VarBinary` aceitam tamanhos de 1 a 8.000.

## varbinary(max)

```csharp
Payload = SqlParam.VarBinaryMax(payload)
```

`VarBinaryMax` usa `SqlDbType.VarBinary` com `Size = -1`.

## Arrays vazios e null

```csharp
EmptyPayload = SqlParam.VarBinary(Array.Empty<byte>(), 1)
MissingPayload = SqlParam.VarBinary(null, 1)
```

Arrays vazios permanecem arrays vazios. Somente `null` é materializado como
`DBNull.Value`.

A biblioteca armazena a referência do `byte[]` fornecido. Ela não copia arrays,
não trunca conteúdo, não faz padding e não valida `value.Length <= size`.
