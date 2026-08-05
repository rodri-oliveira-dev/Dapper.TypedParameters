using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Dapper.TypedParameters.SqlServer;

/// <summary>
/// Creates explicitly typed SQL Server parameters for use with Dapper.
/// </summary>
public static class SqlParam
{
    private const int MaxAnsiLength = 8_000;
    private const int MaxUnicodeLength = 4_000;
    private const byte MaxTemporalScale = 7;

    /// <summary>
    /// Creates a SQL Server <c>varchar</c> parameter.
    /// </summary>
    public static TypedSqlParameter VarChar(string? value, int size) =>
        CreateString(value, SqlDbType.VarChar, size, MaxAnsiLength);

    /// <summary>
    /// Creates a SQL Server <c>nvarchar</c> parameter.
    /// </summary>
    public static TypedSqlParameter NVarChar(string? value, int size) =>
        CreateString(value, SqlDbType.NVarChar, size, MaxUnicodeLength);

    /// <summary>
    /// Creates a SQL Server <c>char</c> parameter.
    /// </summary>
    public static TypedSqlParameter Char(string? value, int size) =>
        CreateString(value, SqlDbType.Char, size, MaxAnsiLength);

    /// <summary>
    /// Creates a SQL Server <c>nchar</c> parameter.
    /// </summary>
    public static TypedSqlParameter NChar(string? value, int size) =>
        CreateString(value, SqlDbType.NChar, size, MaxUnicodeLength);

    /// <summary>
    /// Creates a SQL Server <c>varchar(max)</c> parameter.
    /// </summary>
    public static TypedSqlParameter VarCharMax(string? value) =>
        new(value, SqlDbType.VarChar, size: -1);

    /// <summary>
    /// Creates a SQL Server <c>nvarchar(max)</c> parameter.
    /// </summary>
    public static TypedSqlParameter NVarCharMax(string? value) =>
        new(value, SqlDbType.NVarChar, size: -1);

    /// <summary>
    /// Creates a SQL Server <c>date</c> parameter.
    /// </summary>
    public static TypedSqlParameter Date(DateOnly? value) =>
        new(value, SqlDbType.Date);

    /// <summary>
    /// Creates a SQL Server <c>time</c> parameter.
    /// </summary>
    public static TypedSqlParameter Time(TimeOnly? value, byte scale = MaxTemporalScale) =>
        new(value, SqlDbType.Time, scale: ValidateTemporalScale(scale));

    /// <summary>
    /// Creates a SQL Server <c>datetime</c> parameter.
    /// </summary>
    public static TypedSqlParameter DateTime(DateTime? value) =>
        new(value, SqlDbType.DateTime);

    /// <summary>
    /// Creates a SQL Server <c>smalldatetime</c> parameter.
    /// </summary>
    public static TypedSqlParameter SmallDateTime(DateTime? value) =>
        new(value, SqlDbType.SmallDateTime);

    /// <summary>
    /// Creates a SQL Server <c>datetime2</c> parameter.
    /// </summary>
    public static TypedSqlParameter DateTime2(DateTime? value, byte scale = MaxTemporalScale) =>
        new(value, SqlDbType.DateTime2, scale: ValidateTemporalScale(scale));

    /// <summary>
    /// Creates a SQL Server <c>datetimeoffset</c> parameter.
    /// </summary>
    public static TypedSqlParameter DateTimeOffset(
        DateTimeOffset? value,
        byte scale = MaxTemporalScale) =>
        new(value, SqlDbType.DateTimeOffset, scale: ValidateTemporalScale(scale));

    private static TypedSqlParameter CreateString(
        string? value,
        SqlDbType sqlDbType,
        int size,
        int maximumSize)
    {
        if (size <= 0 || size > maximumSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                size,
                $"Size must be between 1 and {maximumSize}.");
        }

        return new TypedSqlParameter(value, sqlDbType, size);
    }

    private static byte ValidateTemporalScale(byte scale)
    {
        if (scale > MaxTemporalScale)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                scale,
                $"Scale must be between 0 and {MaxTemporalScale}.");
        }

        return scale;
    }
}
