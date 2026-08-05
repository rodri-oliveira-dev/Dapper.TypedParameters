using System;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Dapper.TypedParameters.SqlServer;

/// <summary>
/// Represents an explicitly declared SQL Server parameter that can be consumed by Dapper.
/// </summary>
public sealed class TypedSqlParameter : SqlMapper.ICustomQueryParameter
{
    internal TypedSqlParameter(object? value, SqlDbType sqlDbType, int? size = null)
    {
        Value = value;
        SqlDbType = sqlDbType;
        Size = size;
    }

    /// <summary>
    /// Gets the parameter value before null conversion to <see cref="DBNull.Value"/>.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the SQL Server-specific parameter type.
    /// </summary>
    public SqlDbType SqlDbType { get; }

    /// <summary>
    /// Gets the declared parameter size, or <see langword="null"/> when the type has no size.
    /// A value of <c>-1</c> represents a SQL Server <c>max</c> type.
    /// </summary>
    public int? Size { get; }

    /// <inheritdoc />
    public void AddParameter(IDbCommand command, string name)
    {
        throw new NotImplementedException(
            "SQL Server parameter materialization will be implemented in the next development step.");
    }
}
