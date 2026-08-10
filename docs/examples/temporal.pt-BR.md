# Parâmetros Temporais

[English](temporal.md) | Português (Brasil)

[Voltar ao README](../../README.pt-BR.md) | [Primeiros passos](../getting-started.pt-BR.md)

Factories temporais expõem tipos SQL Server de data e hora usando tipos CLR
modernos quando apropriado.

| Factory | Tipo SQL Server | Tipo CLR |
| --- | --- | --- |
| `SqlParam.Date(value)` | `date` | `DateOnly?` |
| `SqlParam.Time(value, scale)` | `time(scale)` | `TimeOnly?` |
| `SqlParam.DateTime(value)` | `datetime` | `DateTime?` |
| `SqlParam.SmallDateTime(value)` | `smalldatetime` | `DateTime?` |
| `SqlParam.DateTime2(value, scale)` | `datetime2(scale)` | `DateTime?` |
| `SqlParam.DateTimeOffset(value, scale)` | `datetimeoffset(scale)` | `DateTimeOffset?` |

## date e time

```csharp
await connection.ExecuteAsync(
    """
    INSERT INTO dbo.Events (EventDate, EventTime)
    VALUES (@EventDate, @EventTime);
    """,
    new
    {
        EventDate = SqlParam.Date(new DateOnly(2026, 8, 10)),
        EventTime = SqlParam.Time(new TimeOnly(9, 30), scale: 0)
    });
```

`DateOnly` é materializado como `DateTime` à meia-noite. `TimeOnly` é
materializado como `TimeSpan`.

## datetime, smalldatetime, datetime2

```csharp
CreatedAt = SqlParam.DateTime(createdAt)
RoundedAt = SqlParam.SmallDateTime(roundedAt)
PublishedAt = SqlParam.DateTime2(publishedAt, scale: 7)
```

Valores `DateTime` são passados sem alteração de `DateTime.Kind`.

## datetimeoffset

```csharp
OccurredAt = SqlParam.DateTimeOffset(
    new DateTimeOffset(2026, 8, 10, 9, 30, 0, TimeSpan.FromHours(-3)),
    scale: 7)
```

Valores `DateTimeOffset` não são normalizados para UTC e offsets não são
alterados pela biblioteca.

## Escala

`time`, `datetime2` e `datetimeoffset` aceitam valores de `scale` de 0 a 7 e
usam 7 por padrão.

A biblioteca não faz parsing de strings, não arredonda manualmente, não
normaliza time zones e não valida toda a faixa temporal do SQL Server.
