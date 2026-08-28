using System.Data;
using Npgsql;
using NpgsqlTypes;
using Xunit;

namespace Dapper.TypedParameters.PostgreSql.Tests;

public sealed class TypedPostgresParameterTests
{
    [Theory]
    [MemberData(nameof(PostgresParamTests.ScalarFactories), MemberType = typeof(PostgresParamTests))]
    public void AddParameter_materializes_all_scalar_factories(
        string factoryName,
        NpgsqlDbType expectedNpgsqlDbType)
    {
        var typedParameter = PostgresParamTests.CreateNonNullParameter(factoryName);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Equal("Value", parameter.ParameterName);
        Assert.Equal(expectedNpgsqlDbType, parameter.NpgsqlDbType);
        Assert.Equal(typedParameter.Value, parameter.Value);
    }

    [Fact]
    public void AddParameter_converts_null_to_db_null()
    {
        var typedParameter = PostgresParam.Text(null);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Equal(DBNull.Value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Text, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_adds_bytea_parameter()
    {
        byte[] value = [0x01, 0x02];
        var typedParameter = PostgresParam.Bytea(value);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Bytea, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_preserves_empty_binary_array()
    {
        byte[] value = [];
        var typedParameter = PostgresParam.Bytea(value);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Empty(Assert.IsType<byte[]>(parameter.Value));
        Assert.Equal(NpgsqlDbType.Bytea, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_materializes_array_parameter()
    {
        int[] value = [1, 2, 3];
        var typedParameter = PostgresParam.Array(value, NpgsqlDbType.Integer);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Ids");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Integer, parameter.NpgsqlDbType);
        Assert.Equal(ParameterDirection.Input, parameter.Direction);
    }

    [Fact]
    public void AddParameter_materializes_empty_array_parameter()
    {
        string[] value = [];
        var typedParameter = PostgresParam.Array(value, NpgsqlDbType.Text);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Names");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Empty(Assert.IsAssignableFrom<IList<string>>(parameter.Value));
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Text, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_materializes_null_array_parameter_with_explicit_array_type()
    {
        var typedParameter = PostgresParam.Array<Guid>(null, NpgsqlDbType.Uuid);
        using var command = new NpgsqlCommand();

        typedParameter.AddParameter(command, "Ids");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Equal(DBNull.Value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Uuid, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_reuses_existing_parameter()
    {
        var typedParameter = PostgresParam.Text("active");
        using var command = new NpgsqlCommand();
        var existing = command.Parameters.Add("Status", NpgsqlDbType.Boolean);

        typedParameter.AddParameter(command, "Status");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal("active", parameter.Value);
        Assert.Equal(NpgsqlDbType.Text, parameter.NpgsqlDbType);
        Assert.Equal(ParameterDirection.Input, parameter.Direction);
    }

    [Fact]
    public void AddParameter_updates_existing_value()
    {
        var typedParameter = PostgresParam.Integer(42);
        using var command = new NpgsqlCommand();
        var existing = command.Parameters.Add("Value", NpgsqlDbType.Integer);
        existing.Value = 1;

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal(42, parameter.Value);
        Assert.Equal(NpgsqlDbType.Integer, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_replaces_existing_type_metadata_with_explicit_contract()
    {
        var typedParameter = PostgresParam.BigInt(42);
        using var command = new NpgsqlCommand();
        var existing = command.Parameters.Add("Value", NpgsqlDbType.Text);
        existing.Value = "42";

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal(42L, parameter.Value);
        Assert.Equal(NpgsqlDbType.Bigint, parameter.NpgsqlDbType);
    }

    [Fact]
    public void AddParameter_resets_existing_direction_to_input()
    {
        var typedParameter = PostgresParam.Boolean(true);
        using var command = new NpgsqlCommand();
        var existing = command.Parameters.Add("Value", NpgsqlDbType.Boolean);
        existing.Direction = ParameterDirection.Output;

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal(ParameterDirection.Input, parameter.Direction);
    }

    [Fact]
    public void AddParameter_resets_unsupported_scalar_metadata_when_reusing_parameter()
    {
        var typedParameter = PostgresParam.VarChar("longer");
        using var command = new NpgsqlCommand();
        var existing = command.Parameters.Add("Value", NpgsqlDbType.Varchar);
        existing.Size = 3;
        existing.Precision = 10;
        existing.Scale = 2;

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<NpgsqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal("longer", parameter.Value);
        Assert.Equal(NpgsqlDbType.Varchar, parameter.NpgsqlDbType);
        Assert.Equal(0, parameter.Size);
        Assert.Equal(0, parameter.Precision);
        Assert.Equal(0, parameter.Scale);
    }

    [Fact]
    public void AddParameter_rejects_null_command()
    {
        var typedParameter = PostgresParam.Text("value");

        var exception = Assert.Throws<ArgumentNullException>(
            () => typedParameter.AddParameter(null!, "Value"));

        Assert.Equal("command", exception.ParamName);
    }

    [Fact]
    public void AddParameter_rejects_null_name()
    {
        var typedParameter = PostgresParam.Text("value");
        using var command = new NpgsqlCommand();

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
        var typedParameter = PostgresParam.Text("value");
        using var command = new NpgsqlCommand();

        Assert.Throws<ArgumentException>(
            () => typedParameter.AddParameter(command, name));
    }

    [Fact]
    public void AddParameter_rejects_non_postgresql_command()
    {
        var typedParameter = PostgresParam.Text("value");
        using var command = new UnsupportedDbCommand();

        var exception = Assert.Throws<NotSupportedException>(
            () => typedParameter.AddParameter(command, "Value"));

        Assert.Contains(nameof(TypedPostgresParameter), exception.Message);
        Assert.Contains(typeof(NpgsqlCommand).FullName!, exception.Message);
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
