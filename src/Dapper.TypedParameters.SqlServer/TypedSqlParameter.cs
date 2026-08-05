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
    internal TypedSqlParameter(
        object? value,
        SqlDbType sqlDbType,
        int? size = null,
        byte? precision = null,
        byte? scale = null)
    {
        Value = value;
        SqlDbType = sqlDbType;
        Size = size;
        Precision = precision;
        Scale = scale;
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

    /// <summary>
    /// Gets the declared parameter precision, or <see langword="null"/> when the type has no declared precision.
    /// </summary>
    public byte? Precision { get; }

    /// <summary>
    /// Gets the declared parameter scale, or <see langword="null"/> when the type has no declared scale.
    /// </summary>
    public byte? Scale { get; }

    /// <inheritdoc />
    public void AddParameter(IDbCommand command, string name)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Parameter name cannot be null, empty, or whitespace.",
                nameof(name));
        }

        if (command is not SqlCommand sqlCommand)
        {
            throw new NotSupportedException(
                $"{nameof(TypedSqlParameter)} requires a " +
                $"{typeof(SqlCommand).FullName}. " +
                $"The received command type was {command.GetType().FullName}.");
        }

        var parameter = GetOrCreateParameter(sqlCommand, name);

        parameter.Value = Value ?? DBNull.Value;
        parameter.SqlDbType = SqlDbType;

        if (Size.HasValue)
        {
            parameter.Size = Size.Value;
        }

        if (Precision.HasValue)
        {
            parameter.Precision = Precision.Value;
        }

        if (Scale.HasValue)
        {
            parameter.Scale = Scale.Value;
        }
    }

    private static SqlParameter GetOrCreateParameter(
        SqlCommand command,
        string name)
    {
        if (command.Parameters.Contains(name))
        {
            return command.Parameters[name];
        }

        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        command.Parameters.Add(parameter);

        return parameter;
    }
}
