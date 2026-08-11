# Temporal Parameters

[English](temporal.md) | [Português (Brasil)](temporal.pt-BR.md)

[Back to README](../../README.md) | [Getting started](../getting-started.md)

Temporal factories expose SQL Server date and time types using modern CLR types
where appropriate.

| Factory | SQL Server type | CLR type |
| --- | --- | --- |
| `SqlParam.Date(value)` | `date` | `DateOnly?` |
| `SqlParam.Time(value, scale)` | `time(scale)` | `TimeOnly?` |
| `SqlParam.DateTime(value)` | `datetime` | `DateTime?` |
| `SqlParam.SmallDateTime(value)` | `smalldatetime` | `DateTime?` |
| `SqlParam.DateTime2(value, scale)` | `datetime2(scale)` | `DateTime?` |
| `SqlParam.DateTimeOffset(value, scale)` | `datetimeoffset(scale)` | `DateTimeOffset?` |

## date and time

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

`DateOnly` is materialized as `DateTime` at midnight. `TimeOnly` is materialized
as `TimeSpan`.

## datetime, smalldatetime, datetime2

```csharp
CreatedAt = SqlParam.DateTime(createdAt)
RoundedAt = SqlParam.SmallDateTime(roundedAt)
PublishedAt = SqlParam.DateTime2(publishedAt, scale: 7)
```

`DateTime` values are passed without changing `DateTime.Kind`.

## datetimeoffset

```csharp
OccurredAt = SqlParam.DateTimeOffset(
    new DateTimeOffset(2026, 8, 10, 9, 30, 0, TimeSpan.FromHours(-3)),
    scale: 7)
```

`DateTimeOffset` values are not normalized to UTC and offsets are not changed by
the library.

## Scale

`time`, `datetime2`, and `datetimeoffset` accept `scale` values from 0 to 7 and
default to 7.

The library does not parse strings, round manually, normalize time zones, or
validate the complete SQL Server temporal range.
