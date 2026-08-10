using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.Tests;

public sealed class TableValuedSqlParameterTests
{
    [Fact]
    public void TableValued_creates_expected_contract()
    {
        using var table = CreateItemsTable();

        var parameter = SqlParam.TableValued("dbo.ItemList", table);
        var tvp = Assert.IsType<TableValuedSqlParameter>(parameter);

        Assert.IsAssignableFrom<SqlMapper.ICustomQueryParameter>(parameter);
        Assert.Equal("dbo.ItemList", tvp.TypeName);
        Assert.Same(table, tvp.Value);
        Assert.Equal(SqlDbType.Structured, tvp.SqlDbType);
        Assert.Equal(ParameterDirection.Input, tvp.Direction);
    }

    [Fact]
    public void TableValued_accepts_filled_data_table()
    {
        using var table = CreateItemsTable();
        table.Rows.Add(1, "First");

        var parameter = Assert.IsType<TableValuedSqlParameter>(
            SqlParam.TableValued("dbo.ItemList", table));

        Assert.Same(table, parameter.Value);
        Assert.Single(parameter.Value.Rows.Cast<DataRow>());
    }

    [Fact]
    public void TableValued_accepts_empty_data_table()
    {
        using var table = CreateItemsTable();

        var parameter = Assert.IsType<TableValuedSqlParameter>(
            SqlParam.TableValued("dbo.ItemList", table));

        Assert.Same(table, parameter.Value);
        Assert.Empty(parameter.Value.Rows.Cast<DataRow>());
    }

    [Fact]
    public void TableValued_rejects_null_data_table()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => SqlParam.TableValued("dbo.ItemList", null!));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void TableValued_rejects_null_type_name()
    {
        using var table = CreateItemsTable();

        var exception = Assert.Throws<ArgumentNullException>(
            () => SqlParam.TableValued(null!, table));

        Assert.Equal("typeName", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void TableValued_rejects_empty_or_whitespace_type_name(
        string typeName)
    {
        using var table = CreateItemsTable();

        var exception = Assert.Throws<ArgumentException>(
            () => SqlParam.TableValued(typeName, table));

        Assert.Equal("typeName", exception.ParamName);
    }

    [Fact]
    public void AddParameter_materializes_table_valued_parameter()
    {
        using var table = CreateItemsTable();
        table.Rows.Add(1, "First");
        var parameter = SqlParam.TableValued("dbo.ItemList", table);
        using var command = new SqlCommand();

        parameter.AddParameter(command, "Items");

        var sqlParameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Equal("Items", sqlParameter.ParameterName);
        Assert.Same(table, sqlParameter.Value);
        Assert.Equal(SqlDbType.Structured, sqlParameter.SqlDbType);
        Assert.Equal("dbo.ItemList", sqlParameter.TypeName);
        Assert.Equal(ParameterDirection.Input, sqlParameter.Direction);
        Assert.Equal(0, sqlParameter.Size);
        Assert.Equal(0, sqlParameter.Precision);
        Assert.Equal(0, sqlParameter.Scale);
    }

    [Fact]
    public void AddParameter_materializes_empty_table()
    {
        using var table = CreateItemsTable();
        var parameter = SqlParam.TableValued("dbo.ItemList", table);
        using var command = new SqlCommand();

        parameter.AddParameter(command, "Items");

        var sqlParameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(table, sqlParameter.Value);
        Assert.Empty(((DataTable)sqlParameter.Value).Rows.Cast<DataRow>());
    }

    [Fact]
    public void AddParameter_resets_scalar_metadata_when_reusing_parameter()
    {
        using var table = CreateItemsTable();
        var parameter = SqlParam.TableValued("dbo.ItemList", table);
        using var command = new SqlCommand();
        var existing = command.Parameters.Add("Items", SqlDbType.VarChar, 20);
        existing.Direction = ParameterDirection.Output;
        existing.Precision = 18;
        existing.Scale = 2;

        parameter.AddParameter(command, "Items");

        var sqlParameter = Assert.Single(command.Parameters.Cast<SqlParameter>());
        Assert.Same(existing, sqlParameter);
        Assert.Equal(SqlDbType.Structured, sqlParameter.SqlDbType);
        Assert.Equal(ParameterDirection.Input, sqlParameter.Direction);
        Assert.Equal(0, sqlParameter.Size);
        Assert.Equal(0, sqlParameter.Precision);
        Assert.Equal(0, sqlParameter.Scale);
    }

    [Fact]
    public void AddParameter_rejects_non_sql_server_command()
    {
        using var table = CreateItemsTable();
        var parameter = SqlParam.TableValued("dbo.ItemList", table);
        using var command = new UnsupportedDbCommand();

        var exception = Assert.Throws<NotSupportedException>(
            () => parameter.AddParameter(command, "Items"));

        Assert.Contains(typeof(SqlCommand).FullName!, exception.Message);
        Assert.Contains(typeof(UnsupportedDbCommand).FullName!, exception.Message);
    }

    [Fact]
    public void TableValued_parameter_does_not_expose_scalar_metadata_or_output_apis()
    {
        var type = typeof(TableValuedSqlParameter);

        Assert.Null(type.GetProperty("Size"));
        Assert.Null(type.GetProperty("Precision"));
        Assert.Null(type.GetProperty("Scale"));
        Assert.Null(type.GetProperty("OutputValue"));
        Assert.Null(type.GetMethod("AsOutput", Type.EmptyTypes));
        Assert.Null(type.GetMethod("AsInputOutput", Type.EmptyTypes));
        Assert.Null(type.GetMethod("GetValue", Type.EmptyTypes));
    }

    private static DataTable CreateItemsTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        return table;
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
