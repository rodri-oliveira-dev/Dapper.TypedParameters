using System.Data;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.Tests;

public sealed class TypedSqlParameterTests
{
    [Fact]
    public void AddParameter_adds_parameter_with_declared_metadata()
    {
        var typedParameter = SqlParam.VarChar("12345678901", 11);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Document");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal("Document", parameter.ParameterName);
        Assert.Equal("12345678901", parameter.Value);
        Assert.Equal(SqlDbType.VarChar, parameter.SqlDbType);
        Assert.Equal(11, parameter.Size);
    }

    [Fact]
    public void AddParameter_converts_null_to_db_null()
    {
        var typedParameter = SqlParam.NVarChar(null, 150);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Name");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(DBNull.Value, parameter.Value);
        Assert.Equal(SqlDbType.NVarChar, parameter.SqlDbType);
        Assert.Equal(150, parameter.Size);
    }

    [Fact]
    public void AddParameter_configures_max_size()
    {
        var typedParameter = SqlParam.VarCharMax("payload");
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(-1, parameter.Size);
    }

    [Fact]
    public void AddParameter_reuses_existing_parameter()
    {
        var typedParameter = SqlParam.VarChar("active", 20);
        using var command = new SqlCommand();
        var existing = command.Parameters.Add("Status", SqlDbType.NVarChar, 100);

        typedParameter.AddParameter(command, "Status");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal("active", parameter.Value);
        Assert.Equal(SqlDbType.VarChar, parameter.SqlDbType);
        Assert.Equal(20, parameter.Size);
    }

    [Fact]
    public void AddParameter_rejects_null_command()
    {
        var typedParameter = SqlParam.VarChar("value", 20);

        var exception = Assert.Throws<ArgumentNullException>(
            () => typedParameter.AddParameter(null!, "Value"));

        Assert.Equal("command", exception.ParamName);
    }

    [Fact]
    public void AddParameter_rejects_null_name()
    {
        var typedParameter = SqlParam.VarChar("value", 20);
        using var command = new SqlCommand();

        var exception = Assert.Throws<ArgumentException>(
            () => typedParameter.AddParameter(command, null!));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void AddParameter_rejects_empty_or_whitespace_name(string name)
    {
        var typedParameter = SqlParam.VarChar("value", 20);
        using var command = new SqlCommand();

        Assert.Throws<ArgumentException>(
            () => typedParameter.AddParameter(command, name));
    }

    [Fact]
    public void AddParameter_rejects_non_sql_server_command()
    {
        var typedParameter = SqlParam.VarChar("value", 20);
        using var command = new UnsupportedDbCommand();

        var exception = Assert.Throws<NotSupportedException>(
            () => typedParameter.AddParameter(command, "Value"));

        Assert.Contains(typeof(SqlCommand).FullName!, exception.Message);
        Assert.Contains(typeof(UnsupportedDbCommand).FullName!, exception.Message);
    }

#nullable disable
    private sealed class UnsupportedDbCommand : IDbCommand
    {
        public string CommandText { get; set; } = string.Empty;

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDbConnection Connection { get; set; }

        public IDataParameterCollection Parameters =>
            throw new NotSupportedException();

        public IDbTransaction Transaction { get; set; }

        public UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel()
        {
        }

        public IDbDataParameter CreateParameter() =>
            throw new NotSupportedException();

        public void Dispose()
        {
        }

        public int ExecuteNonQuery() =>
            throw new NotSupportedException();

        public IDataReader ExecuteReader() =>
            throw new NotSupportedException();

        public IDataReader ExecuteReader(CommandBehavior behavior) =>
            throw new NotSupportedException();

        public object ExecuteScalar() =>
            throw new NotSupportedException();

        public void Prepare()
        {
        }
    }
#nullable restore
}
