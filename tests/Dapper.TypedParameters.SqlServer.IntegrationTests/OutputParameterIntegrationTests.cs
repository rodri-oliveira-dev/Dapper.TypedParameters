using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.IntegrationTests;

[Collection(SqlServerIntegrationCollectionNames.Default)]
public sealed class OutputParameterIntegrationTests
{
    private readonly SqlServerContainerFixture fixture;

    public OutputParameterIntegrationTests(SqlServerContainerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_varchar()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_varchar));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Result varchar(20) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Result = 'varchar-output';
            END
            """,
            async fullProcedureName =>
            {
                var result = SqlParam.VarChar(null, 20).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Result = result },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal("varchar-output", result.GetValue<string>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_nvarchar()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_nvarchar));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Result nvarchar(20) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Result = N'nvarchar-output';
            END
            """,
            async fullProcedureName =>
            {
                var result = SqlParam.NVarChar(null, 20).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Result = result },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal("nvarchar-output", result.GetValue<string>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_int()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_int));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Result int OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Result = 42;
            END
            """,
            async fullProcedureName =>
            {
                var result = SqlParam.Int(null).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Result = result },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal(42, result.GetValue<int>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_decimal()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_decimal));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Amount decimal(18, 2) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Amount = 123.45;
            END
            """,
            async fullProcedureName =>
            {
                var amount = SqlParam.Decimal(null, 18, 2).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Amount = amount },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal(123.45M, amount.GetValue<decimal>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_datetime2()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_datetime2));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Moment datetime2(7) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Moment = '2026-08-05T12:34:56.1234567';
            END
            """,
            async fullProcedureName =>
            {
                var moment = SqlParam.DateTime2(null).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Moment = moment },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal(
                    new DateTime(2026, 8, 5, 12, 34, 56).AddTicks(1_234_567),
                    moment.GetValue<DateTime>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_uniqueidentifier()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_uniqueidentifier));
        var id = Guid.Parse("58a0a1bd-2158-49f3-9a7e-c94cb4f893b9");

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            $"""
            @Id uniqueidentifier OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Id = '{id:D}';
            END
            """,
            async fullProcedureName =>
            {
                var result = SqlParam.UniqueIdentifier(null).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Id = result },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal(id, result.GetValue<Guid>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_output_null()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_output_null));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Result nvarchar(20) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Result = NULL;
            END
            """,
            async fullProcedureName =>
            {
                var result = SqlParam.NVarChar(null, 20).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Result = result },
                    commandType: CommandType.StoredProcedure);

                Assert.Null(result.OutputValue);
                Assert.Null(result.GetValue<string?>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_input_output_int()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_input_output_int));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Counter int OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Counter = @Counter + 1;
            END
            """,
            async fullProcedureName =>
            {
                var counter = SqlParam.Int(41).AsInputOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Counter = counter },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal(42, counter.GetValue<int>());
            });
    }

    [Fact]
    public async Task ExecuteAsync_reads_multiple_outputs()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        var procedureName = CreateProcedureName(nameof(ExecuteAsync_reads_multiple_outputs));

        await ExecuteWithProcedureAsync(
            connection,
            procedureName,
            """
            @Code int OUTPUT,
            @Message varchar(20) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @Code = 7;
                SET @Message = 'complete';
            END
            """,
            async fullProcedureName =>
            {
                var code = SqlParam.Int(null).AsOutput();
                var message = SqlParam.VarChar(null, 20).AsOutput();

                await connection.ExecuteAsync(
                    fullProcedureName,
                    new { Code = code, Message = message },
                    commandType: CommandType.StoredProcedure);

                Assert.Equal(7, code.GetValue<int>());
                Assert.Equal("complete", message.GetValue<string>());
            });
    }

    private static string CreateProcedureName(string testName) =>
        $"DapperTypedParameters_{testName}_{Guid.NewGuid():N}";

    private static async Task ExecuteWithProcedureAsync(
        SqlConnection connection,
        string procedureName,
        string procedureBody,
        Func<string, Task> executeAsync)
    {
        var fullProcedureName = $"dbo.{procedureName}";
        var quotedProcedureName = $"[dbo].[{procedureName}]";

        await connection.ExecuteAsync(
            $"CREATE PROCEDURE {quotedProcedureName} {procedureBody}");

        try
        {
            await executeAsync(fullProcedureName);
        }
        finally
        {
            await connection.ExecuteAsync(
                $"DROP PROCEDURE IF EXISTS {quotedProcedureName};");
        }
    }
}
