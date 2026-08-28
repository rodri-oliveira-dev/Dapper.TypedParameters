using System;
using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Dapper.TypedParameters.PostgreSql;

/// <summary>
/// Represents an explicitly declared PostgreSQL parameter that can be consumed by Dapper.
/// </summary>
public sealed class TypedPostgresParameter : SqlMapper.ICustomQueryParameter
{
    internal TypedPostgresParameter(object? value, NpgsqlDbType npgsqlDbType)
    {
        Value = value;
        NpgsqlDbType = npgsqlDbType;
    }

    /// <summary>
    /// Gets the parameter value before null conversion to <see cref="DBNull.Value"/>.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the Npgsql-specific PostgreSQL parameter type.
    /// </summary>
    public NpgsqlDbType NpgsqlDbType { get; }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty, or whitespace.</exception>
    /// <exception cref="NotSupportedException"><paramref name="command"/> is not an <see cref="NpgsqlCommand"/>.</exception>
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

        if (command is not NpgsqlCommand npgsqlCommand)
        {
            throw new NotSupportedException(
                $"{nameof(TypedPostgresParameter)} requires a " +
                $"{typeof(NpgsqlCommand).FullName}. " +
                $"The received command type was {command.GetType().FullName}.");
        }

        var parameter = GetOrCreateParameter(npgsqlCommand, name);

        parameter.Value = Value ?? DBNull.Value;
        parameter.NpgsqlDbType = NpgsqlDbType;
        parameter.Direction = ParameterDirection.Input;
        parameter.Size = 0;
        parameter.Precision = 0;
        parameter.Scale = 0;
    }

    private static NpgsqlParameter GetOrCreateParameter(
        NpgsqlCommand command,
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
