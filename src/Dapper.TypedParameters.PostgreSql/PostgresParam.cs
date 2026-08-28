using System;
using NpgsqlTypes;

namespace Dapper.TypedParameters.PostgreSql;

/// <summary>
/// Creates explicitly typed PostgreSQL parameters for use with Dapper.
/// </summary>
public static class PostgresParam
{
    /// <summary>
    /// Creates a PostgreSQL <c>text</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Text(string? value) =>
        new(value, NpgsqlDbType.Text);

    /// <summary>
    /// Creates a PostgreSQL <c>character varying</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter VarChar(string? value) =>
        new(value, NpgsqlDbType.Varchar);

    /// <summary>
    /// Creates a PostgreSQL <c>character</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Char(string? value) =>
        new(value, NpgsqlDbType.Char);

    /// <summary>
    /// Creates a PostgreSQL <c>boolean</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Boolean(bool? value) =>
        new(value, NpgsqlDbType.Boolean);

    /// <summary>
    /// Creates a PostgreSQL <c>smallint</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter SmallInt(short? value) =>
        new(value, NpgsqlDbType.Smallint);

    /// <summary>
    /// Creates a PostgreSQL <c>integer</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Integer(int? value) =>
        new(value, NpgsqlDbType.Integer);

    /// <summary>
    /// Creates a PostgreSQL <c>bigint</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter BigInt(long? value) =>
        new(value, NpgsqlDbType.Bigint);

    /// <summary>
    /// Creates a PostgreSQL <c>real</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Real(float? value) =>
        new(value, NpgsqlDbType.Real);

    /// <summary>
    /// Creates a PostgreSQL <c>double precision</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Double(double? value) =>
        new(value, NpgsqlDbType.Double);

    /// <summary>
    /// Creates a PostgreSQL <c>numeric</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Numeric(decimal? value) =>
        new(value, NpgsqlDbType.Numeric);

    /// <summary>
    /// Creates a PostgreSQL <c>money</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Money(decimal? value) =>
        new(value, NpgsqlDbType.Money);

    /// <summary>
    /// Creates a PostgreSQL <c>json</c> parameter from caller-provided JSON text.
    /// </summary>
    public static TypedPostgresParameter Json(string? value) =>
        new(value, NpgsqlDbType.Json);

    /// <summary>
    /// Creates a PostgreSQL <c>jsonb</c> parameter from caller-provided JSON text.
    /// </summary>
    public static TypedPostgresParameter Jsonb(string? value) =>
        new(value, NpgsqlDbType.Jsonb);

    /// <summary>
    /// Creates a PostgreSQL <c>uuid</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Uuid(Guid? value) =>
        new(value, NpgsqlDbType.Uuid);

    /// <summary>
    /// Creates a PostgreSQL <c>bytea</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Bytea(byte[]? value) =>
        new(value, NpgsqlDbType.Bytea);

    /// <summary>
    /// Creates a PostgreSQL <c>date</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Date(DateOnly? value) =>
        new(value, NpgsqlDbType.Date);

    /// <summary>
    /// Creates a PostgreSQL <c>time without time zone</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Time(TimeOnly? value) =>
        new(value, NpgsqlDbType.Time);

    /// <summary>
    /// Creates a PostgreSQL <c>timestamp without time zone</c> wall-clock parameter.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> has <see cref="DateTimeKind.Utc"/>.</exception>
    public static TypedPostgresParameter Timestamp(DateTime? value)
    {
        if (value.HasValue && value.GetValueOrDefault().Kind == DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Timestamp requires a local or unspecified wall-clock DateTime. Use TimestampTz for UTC instants.",
                nameof(value));
        }

        return new(value, NpgsqlDbType.Timestamp);
    }

    /// <summary>
    /// Creates a PostgreSQL <c>timestamp with time zone</c> parameter representing a UTC instant.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not <see cref="DateTimeKind.Utc"/>.</exception>
    public static TypedPostgresParameter TimestampTz(DateTime? value)
    {
        if (value.HasValue && value.GetValueOrDefault().Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "TimestampTz requires a UTC DateTime and does not convert Local or Unspecified values.",
                nameof(value));
        }

        return new(value, NpgsqlDbType.TimestampTz);
    }

    /// <summary>
    /// Creates a PostgreSQL <c>interval</c> parameter from a <see cref="TimeSpan"/> value.
    /// </summary>
    /// <remarks>
    /// PostgreSQL intervals can contain month and year components that <see cref="TimeSpan"/> cannot represent.
    /// </remarks>
    public static TypedPostgresParameter Interval(TimeSpan? value) =>
        new(value, NpgsqlDbType.Interval);
}
