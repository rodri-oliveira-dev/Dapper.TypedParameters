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
        byte? scale = null,
        ParameterDirection direction = ParameterDirection.Input)
    {
        Value = value;
        SqlDbType = sqlDbType;
        Size = size;
        Precision = precision;
        Scale = scale;
        Direction = direction;
    }

    private SqlParameter? materializedParameter;

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

    /// <summary>
    /// Gets the parameter direction.
    /// </summary>
    public ParameterDirection Direction { get; }

    /// <summary>
    /// Gets the value assigned by SQL Server after command execution.
    /// </summary>
    public object? OutputValue
    {
        get
        {
            var value = GetMaterializedOutputValue();

            return value == DBNull.Value ? null : value;
        }
    }

    /// <summary>
    /// Creates an equivalent parameter configured as an output parameter.
    /// </summary>
    public TypedSqlParameter AsOutput() =>
        WithDirection(ParameterDirection.Output);

    /// <summary>
    /// Creates an equivalent parameter configured as an input/output parameter.
    /// </summary>
    public TypedSqlParameter AsInputOutput() =>
        WithDirection(ParameterDirection.InputOutput);

    /// <summary>
    /// Gets the output value using CLR casting rules without silent conversion.
    /// </summary>
    public T? GetValue<T>()
    {
        var value = OutputValue;

        if (value is null)
        {
            if (IsNonNullableValueType(typeof(T)))
            {
                throw new InvalidOperationException(
                    "The output parameter value is database null and cannot be " +
                    $"returned as non-nullable {typeof(T).FullName}.");
            }

            return default;
        }

        var requestedType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (!requestedType.IsInstanceOfType(value))
        {
            throw new InvalidCastException(
                "The output parameter value cannot be returned as " +
                $"{typeof(T).FullName}. The actual value type is " +
                $"{value.GetType().FullName}.");
        }

        return (T)value;
    }

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

        parameter.Value = MaterializeValue(Value) ?? DBNull.Value;
        parameter.SqlDbType = SqlDbType;
        parameter.Direction = Direction;

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

        materializedParameter = parameter;
    }

    private TypedSqlParameter WithDirection(ParameterDirection direction) =>
        new(Value, SqlDbType, Size, Precision, Scale, direction);

    private object? GetMaterializedOutputValue()
    {
        if (materializedParameter is null)
        {
            throw new InvalidOperationException(
                "The SQL parameter has not been materialized by Dapper. " +
                "Pass this instance to Dapper, wait for command execution to " +
                "complete, and then read the output value.");
        }

        return materializedParameter.Value;
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

    private static object? MaterializeValue(object? value) =>
        value switch
        {
            DateOnly date => date.ToDateTime(TimeOnly.MinValue),
            TimeOnly time => time.ToTimeSpan(),
            _ => value
        };

    private static bool IsNonNullableValueType(Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) is null;
}
