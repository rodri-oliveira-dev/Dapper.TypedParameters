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
    /// Creates a PostgreSQL <c>money</c> parameter with explicit PostgreSQL parameter metadata.
    /// </summary>
    public static TypedPostgresParameter Money(decimal? value) =>
        new(value, NpgsqlDbType.Money);

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
}
