using System.Data;

namespace Dapper.TypedParameters.SqlServer;

/// <summary>
/// Creates explicitly typed SQL Server parameters for use with Dapper.
/// </summary>
public static class SqlParam
{
    private const int MaxAnsiLength = 8_000;
    private const int MaxUnicodeLength = 4_000;

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
}
