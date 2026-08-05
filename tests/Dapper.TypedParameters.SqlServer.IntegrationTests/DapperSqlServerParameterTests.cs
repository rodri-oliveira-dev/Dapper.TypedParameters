using Dapper;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.IntegrationTests;

[Collection(SqlServerIntegrationCollectionNames.Default)]
public sealed class DapperSqlServerParameterTests
{
    private readonly SqlServerContainerFixture fixture;

    public DapperSqlServerParameterTests(SqlServerContainerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task QueryAsync_uses_varchar_parameter_with_declared_sql_type()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string value = await connection.QuerySingleAsync<string>(
            "SELECT CAST(@Document AS varchar(11));",
            new
            {
                Document = SqlParam.VarChar("12345678901", 11)
            });

        string baseType = await connection.QuerySingleAsync<string>(
            "SELECT CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Document, 'BaseType'));",
            new
            {
                Document = SqlParam.VarChar("12345678901", 11)
            });

        Assert.Equal("12345678901", value);
        Assert.Equal("varchar", baseType);
    }

    [Fact]
    public async Task QueryAsync_uses_nvarchar_parameter_with_declared_sql_type()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string value = await connection.QuerySingleAsync<string>(
            "SELECT CAST(@Name AS nvarchar(100));",
            new
            {
                Name = SqlParam.NVarChar("Rodrigo", 100)
            });

        string baseType = await connection.QuerySingleAsync<string>(
            "SELECT CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Name, 'BaseType'));",
            new
            {
                Name = SqlParam.NVarChar("Rodrigo", 100)
            });

        Assert.Equal("Rodrigo", value);
        Assert.Equal("nvarchar", baseType);
    }

    [Fact]
    public async Task ExecuteAsync_inserts_varchar_and_nvarchar_columns_with_multiple_typed_parameters()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE #Documents (Document varchar(11) NOT NULL, Name nvarchar(100) NOT NULL);");

        try
        {
            int affectedRows = await connection.ExecuteAsync(
                "INSERT INTO #Documents (Document, Name) VALUES (@Document, @Name);",
                new
                {
                    Document = SqlParam.VarChar("12345678901", 11),
                    Name = SqlParam.NVarChar("Rodrigo", 100)
                });

            (string Document, string Name) result = await connection.QuerySingleAsync<(string, string)>(
                "SELECT Document, Name FROM #Documents;");

            Assert.Equal(1, affectedRows);
            Assert.Equal("12345678901", result.Document);
            Assert.Equal("Rodrigo", result.Name);
        }
        finally
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS #Documents;");
        }
    }

    [Fact]
    public async Task ExecuteAsync_updates_row_using_typed_parameter()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE #Statuses (Document varchar(11) NOT NULL, Status nvarchar(30) NOT NULL);");

        try
        {
            await connection.ExecuteAsync(
                "INSERT INTO #Statuses (Document, Status) VALUES ('12345678901', N'Pending');");

            int affectedRows = await connection.ExecuteAsync(
                "UPDATE #Statuses SET Status = @Status WHERE Document = @Document;",
                new
                {
                    Document = SqlParam.VarChar("12345678901", 11),
                    Status = SqlParam.NVarChar("Approved", 30)
                });

            string status = await connection.QuerySingleAsync<string>(
                "SELECT Status FROM #Statuses WHERE Document = '12345678901';");

            Assert.Equal(1, affectedRows);
            Assert.Equal("Approved", status);
        }
        finally
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS #Statuses;");
        }
    }

    [Fact]
    public async Task ExecuteAsync_deletes_row_using_typed_parameter()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE #Queue (Document varchar(11) NOT NULL);");

        try
        {
            await connection.ExecuteAsync(
                "INSERT INTO #Queue (Document) VALUES ('12345678901'), ('99999999999');");

            int affectedRows = await connection.ExecuteAsync(
                "DELETE FROM #Queue WHERE Document = @Document;",
                new
                {
                    Document = SqlParam.VarChar("12345678901", 11)
                });

            int remainingRows = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM #Queue;");

            Assert.Equal(1, affectedRows);
            Assert.Equal(1, remainingRows);
        }
        finally
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS #Queue;");
        }
    }

    [Fact]
    public async Task QueryAsync_sends_null_value_as_database_null()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        bool isNull = await connection.QuerySingleAsync<bool>(
            "SELECT CONVERT(bit, CASE WHEN @Name IS NULL THEN 1 ELSE 0 END);",
            new
            {
                Name = SqlParam.NVarChar(null, 100)
            });

        Assert.True(isNull);
    }

    [Fact]
    public async Task QueryAsync_uses_varchar_max_parameter()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string payload = new('A', 8_100);

        string value = await connection.QuerySingleAsync<string>(
            "SELECT CAST(@Payload AS varchar(max));",
            new
            {
                Payload = SqlParam.VarCharMax(payload)
            });

        Assert.Equal(payload, value);
    }

    [Fact]
    public async Task QueryAsync_uses_nvarchar_max_parameter()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string payload = new('Á', 4_100);

        string value = await connection.QuerySingleAsync<string>(
            "SELECT CAST(@Payload AS nvarchar(max));",
            new
            {
                Payload = SqlParam.NVarCharMax(payload)
            });

        Assert.Equal(payload, value);
    }
}
