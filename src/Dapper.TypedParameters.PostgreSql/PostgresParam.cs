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
    /// Creates a PostgreSQL array parameter with an explicit PostgreSQL element type.
    /// </summary>
    /// <param name="value">The array or list value to send, or <see langword="null"/>.</param>
    /// <param name="elementType">The PostgreSQL type of each array element.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="elementType"/> is not a supported v1 scalar element type.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="elementType"/> already includes array, range, or multirange semantics.
    /// </exception>
    public static TypedPostgresParameter Array<T>(
        IList<T>? value,
        NpgsqlDbType elementType)
    {
        ValidateArrayElementType(elementType);

        return new(value, NpgsqlDbType.Array | elementType);
    }

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

    private static void ValidateArrayElementType(NpgsqlDbType elementType)
    {
        if ((elementType & NpgsqlDbType.Array) != 0 ||
            (elementType & NpgsqlDbType.Range) != 0 ||
            (elementType & NpgsqlDbType.Multirange) != 0)
        {
            throw new ArgumentException(
                "Array element type must not include array, range, or multirange semantics.",
                nameof(elementType));
        }

        if (!IsSupportedArrayElementType(elementType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(elementType),
                elementType,
                "Array element type must be one of the PostgreSQL scalar types supported by this package.");
        }
    }

    private static bool IsSupportedArrayElementType(NpgsqlDbType elementType) =>
        elementType switch
        {
            NpgsqlDbType.Text or
            NpgsqlDbType.Varchar or
            NpgsqlDbType.Char or
            NpgsqlDbType.Boolean or
            NpgsqlDbType.Smallint or
            NpgsqlDbType.Integer or
            NpgsqlDbType.Bigint or
            NpgsqlDbType.Real or
            NpgsqlDbType.Double or
            NpgsqlDbType.Numeric or
            NpgsqlDbType.Money or
            NpgsqlDbType.Json or
            NpgsqlDbType.Jsonb or
            NpgsqlDbType.Uuid or
            NpgsqlDbType.Bytea or
            NpgsqlDbType.Date or
            NpgsqlDbType.Time or
            NpgsqlDbType.Timestamp or
            NpgsqlDbType.TimestampTz or
            NpgsqlDbType.Interval => true,
            _ => false,
        };
}
