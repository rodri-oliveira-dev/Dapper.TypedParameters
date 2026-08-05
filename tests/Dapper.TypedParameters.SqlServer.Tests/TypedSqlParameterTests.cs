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
        Assert.Equal(ParameterDirection.Input, parameter.Direction);
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
    public void AddParameter_configures_output_direction()
    {
        var typedParameter = SqlParam.VarChar(null, 20).AsOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(ParameterDirection.Output, parameter.Direction);
        Assert.Equal(SqlDbType.VarChar, parameter.SqlDbType);
        Assert.Equal(20, parameter.Size);
    }

    [Fact]
    public void AddParameter_configures_input_output_direction()
    {
        var typedParameter = SqlParam.Int(1).AsInputOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(ParameterDirection.InputOutput, parameter.Direction);
        Assert.Equal(1, parameter.Value);
        Assert.Equal(SqlDbType.Int, parameter.SqlDbType);
    }

    [Theory]
    [MemberData(nameof(SqlParamTests.ScalarFactories), MemberType = typeof(SqlParamTests))]
    public void AddParameter_materializes_output_direction_for_all_scalar_factories(
        string factoryName)
    {
        var parameter = SqlParamTests.CreateScalarParameter(factoryName);
        using var command = new SqlCommand();

        parameter.AsOutput().AddParameter(command, "Value");

        var sqlParameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.NotEmpty(factoryName);
        Assert.Equal(ParameterDirection.Output, sqlParameter.Direction);
        Assert.Equal(parameter.SqlDbType, sqlParameter.SqlDbType);
    }

    [Fact]
    public void OutputValue_rejects_read_before_materialization()
    {
        var typedParameter = SqlParam.Int(null).AsOutput();

        var exception = Assert.Throws<InvalidOperationException>(
            () => typedParameter.OutputValue);

        Assert.Contains("has not been materialized", exception.Message);
    }

    [Fact]
    public void GetValue_rejects_read_before_materialization()
    {
        var typedParameter = SqlParam.Int(null).AsOutput();

        var exception = Assert.Throws<InvalidOperationException>(
            () => typedParameter.GetValue<int?>());

        Assert.Contains("has not been materialized", exception.Message);
    }

    [Fact]
    public void OutputValue_reads_materialized_parameter_value()
    {
        var typedParameter = SqlParam.Int(null).AsOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");
        command.Parameters["Value"].Value = 42;

        Assert.Equal(42, typedParameter.OutputValue);
        Assert.Equal(42, typedParameter.GetValue<int>());
    }

    [Fact]
    public void OutputValue_returns_null_for_db_null()
    {
        var typedParameter = SqlParam.NVarChar(null, 20).AsOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");
        command.Parameters["Value"].Value = DBNull.Value;

        Assert.Null(typedParameter.OutputValue);
        Assert.Null(typedParameter.GetValue<string?>());
        Assert.Null(typedParameter.GetValue<int?>());
    }

    [Fact]
    public void GetValue_rejects_db_null_for_non_nullable_value_type()
    {
        var typedParameter = SqlParam.Int(null).AsOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");
        command.Parameters["Value"].Value = DBNull.Value;

        var exception = Assert.Throws<InvalidOperationException>(
            () => typedParameter.GetValue<int>());

        Assert.Contains("database null", exception.Message);
        Assert.Contains(typeof(int).FullName!, exception.Message);
    }

    [Fact]
    public void GetValue_allows_compatible_reference_conversion()
    {
        var typedParameter = SqlParam.VarChar(null, 20).AsOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");
        command.Parameters["Value"].Value = "done";

        Assert.Equal("done", typedParameter.GetValue<string>());
        Assert.Equal("done", typedParameter.GetValue<object>());
    }

    [Fact]
    public void GetValue_rejects_incompatible_conversion()
    {
        var typedParameter = SqlParam.VarChar(null, 20).AsOutput();
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");
        command.Parameters["Value"].Value = "done";

        var exception = Assert.Throws<InvalidCastException>(
            () => typedParameter.GetValue<int>());

        Assert.Contains(typeof(int).FullName!, exception.Message);
        Assert.Contains(typeof(string).FullName!, exception.Message);
    }

    [Fact]
    public void Output_parameter_can_be_reused_non_concurrently()
    {
        var typedParameter = SqlParam.Int(null).AsOutput();
        using var firstCommand = new SqlCommand();
        using var secondCommand = new SqlCommand();

        typedParameter.AddParameter(firstCommand, "Value");
        firstCommand.Parameters["Value"].Value = 1;

        Assert.Equal(1, typedParameter.GetValue<int>());

        typedParameter.AddParameter(secondCommand, "Value");
        secondCommand.Parameters["Value"].Value = 2;

        Assert.Equal(2, typedParameter.GetValue<int>());
    }

    [Fact]
    public void AddParameter_materializes_date_only_as_date_time()
    {
        var value = new DateOnly(2026, 8, 5);
        var typedParameter = SqlParam.Date(value);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(value.ToDateTime(TimeOnly.MinValue), parameter.Value);
        Assert.Equal(SqlDbType.Date, parameter.SqlDbType);
        Assert.Equal(0, parameter.Size);
    }

    [Fact]
    public void AddParameter_materializes_time_only_as_time_span()
    {
        var value = new TimeOnly(12, 34, 56, 789);
        var typedParameter = SqlParam.Time(value, scale: 3);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(value.ToTimeSpan(), parameter.Value);
        Assert.Equal(SqlDbType.Time, parameter.SqlDbType);
        Assert.Equal(0, parameter.Size);
        Assert.Equal((byte)3, parameter.Scale);
    }

    [Fact]
    public void AddParameter_configures_temporal_scale_without_size()
    {
        var typedParameter = SqlParam.DateTime2(
            new DateTime(2026, 8, 5, 12, 34, 56, 789),
            scale: 7);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(SqlDbType.DateTime2, parameter.SqlDbType);
        Assert.Equal(0, parameter.Size);
        Assert.Equal((byte)7, parameter.Scale);
    }

    [Fact]
    public void AddParameter_keeps_date_time_value_kind()
    {
        var value = new DateTime(2026, 8, 5, 12, 34, 56, DateTimeKind.Utc);
        var typedParameter = SqlParam.DateTime2(value);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        var materializedValue = Assert.IsType<DateTime>(parameter.Value);
        Assert.Equal(DateTimeKind.Utc, materializedValue.Kind);
    }

    [Fact]
    public void AddParameter_adds_numeric_parameter_without_size()
    {
        var typedParameter = SqlParam.Int(42);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Value");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal("Value", parameter.ParameterName);
        Assert.Equal(42, parameter.Value);
        Assert.Equal(SqlDbType.Int, parameter.SqlDbType);
        Assert.Equal(0, parameter.Size);
        Assert.Equal(0, parameter.Precision);
        Assert.Equal(0, parameter.Scale);
    }

    [Fact]
    public void AddParameter_adds_decimal_parameter_with_declared_precision_and_scale()
    {
        var typedParameter = SqlParam.Decimal(123.45M, 18, 2);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Amount");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(123.45M, parameter.Value);
        Assert.Equal(SqlDbType.Decimal, parameter.SqlDbType);
        Assert.Equal(0, parameter.Size);
        Assert.Equal((byte)18, parameter.Precision);
        Assert.Equal((byte)2, parameter.Scale);
    }

    [Fact]
    public void AddParameter_converts_numeric_null_to_db_null()
    {
        var typedParameter = SqlParam.Decimal(null, 38, 18);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Amount");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(DBNull.Value, parameter.Value);
        Assert.Equal(SqlDbType.Decimal, parameter.SqlDbType);
        Assert.Equal((byte)38, parameter.Precision);
        Assert.Equal((byte)18, parameter.Scale);
    }

    [Fact]
    public void AddParameter_adds_uniqueidentifier_parameter()
    {
        var id = Guid.Parse("7cdb49ea-c947-4fe1-861b-ddd941a02422");
        var typedParameter = SqlParam.UniqueIdentifier(id);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Id");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(id, parameter.Value);
        Assert.Equal(SqlDbType.UniqueIdentifier, parameter.SqlDbType);
        Assert.Equal(0, parameter.Size);
    }

    [Fact]
    public void AddParameter_converts_uniqueidentifier_null_to_db_null()
    {
        var typedParameter = SqlParam.UniqueIdentifier(null);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Id");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(DBNull.Value, parameter.Value);
        Assert.Equal(SqlDbType.UniqueIdentifier, parameter.SqlDbType);
    }

    [Fact]
    public void AddParameter_adds_binary_parameter_with_declared_size()
    {
        byte[] value = [0x01, 0x02];
        var typedParameter = SqlParam.Binary(value, 2);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.Binary, parameter.SqlDbType);
        Assert.Equal(2, parameter.Size);
    }

    [Fact]
    public void AddParameter_adds_varbinary_parameter_with_declared_size()
    {
        byte[] value = [0x01, 0x02];
        var typedParameter = SqlParam.VarBinary(value, 8_000);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
        Assert.Equal(8_000, parameter.Size);
    }

    [Fact]
    public void AddParameter_adds_varbinary_max_parameter()
    {
        byte[] value = [0x01, 0x02];
        var typedParameter = SqlParam.VarBinaryMax(value);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
        Assert.Equal(-1, parameter.Size);
    }

    [Fact]
    public void AddParameter_preserves_empty_binary_array()
    {
        byte[] value = [];
        var typedParameter = SqlParam.VarBinary(value, 1);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(value, parameter.Value);
        Assert.Empty((byte[])parameter.Value);
    }

    [Fact]
    public void AddParameter_converts_null_binary_array_to_db_null()
    {
        var typedParameter = SqlParam.VarBinary(null, 1);
        using var command = new SqlCommand();

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal(DBNull.Value, parameter.Value);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
        Assert.Equal(1, parameter.Size);
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
    public void AddParameter_reuses_existing_parameter_for_decimal_metadata()
    {
        var typedParameter = SqlParam.Decimal(-123.45M, 18, 2);
        using var command = new SqlCommand();
        var existing = command.Parameters.Add("Amount", SqlDbType.Int);

        typedParameter.AddParameter(command, "Amount");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Equal(-123.45M, parameter.Value);
        Assert.Equal(SqlDbType.Decimal, parameter.SqlDbType);
        Assert.Equal((byte)18, parameter.Precision);
        Assert.Equal((byte)2, parameter.Scale);
    }

    [Fact]
    public void AddParameter_reuses_existing_parameter_for_binary_metadata()
    {
        byte[] value = [0x0A, 0x0B];
        var typedParameter = SqlParam.Binary(value, 2);
        using var command = new SqlCommand();
        var existing = command.Parameters.Add("Payload", SqlDbType.VarBinary, 100);

        typedParameter.AddParameter(command, "Payload");

        var parameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(existing, parameter);
        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.Binary, parameter.SqlDbType);
        Assert.Equal(2, parameter.Size);
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
