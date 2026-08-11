using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Dapper.TypedParameters.SqlServer;

/// <summary>
/// Represents a SQL Server table-valued parameter backed by a <see cref="DataTable"/>.
/// </summary>
public sealed class TableValuedSqlParameter : SqlMapper.ICustomQueryParameter
{
    internal TableValuedSqlParameter(string typeName, DataTable value)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException(
                "Table type name cannot be empty or whitespace.",
                nameof(typeName));
        }

        TypeName = typeName;
        Value = value;
    }

    /// <summary>
    /// Gets the SQL Server user-defined table type name.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the table value before it is assigned to the provider parameter.
    /// </summary>
    public DataTable Value { get; }

    /// <summary>
    /// Gets the SQL Server-specific parameter type.
    /// </summary>
    [SuppressMessage(
        "Major Code Smell",
        "S2325:Methods and properties that don't access instance data should be static",
        Justification = "This member is intentionally preserved as public instance metadata in the frozen 1.0 TVP contract.")]
    public SqlDbType SqlDbType => SqlDbType.Structured;

    /// <summary>
    /// Gets the parameter direction.
    /// </summary>
    [SuppressMessage(
        "Major Code Smell",
        "S2325:Methods and properties that don't access instance data should be static",
        Justification = "This member is intentionally preserved as public instance metadata in the frozen 1.0 TVP contract.")]
    public ParameterDirection Direction => ParameterDirection.Input;

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
                $"{nameof(TableValuedSqlParameter)} requires a " +
                $"{typeof(SqlCommand).FullName}. " +
                $"The received command type was {command.GetType().FullName}.");
        }

        var parameter = GetOrCreateParameter(sqlCommand, name);

        parameter.Value = Value;
        parameter.SqlDbType = SqlDbType.Structured;
        parameter.TypeName = TypeName;
        parameter.Direction = ParameterDirection.Input;
        parameter.Size = 0;
        parameter.Precision = 0;
        parameter.Scale = 0;
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
