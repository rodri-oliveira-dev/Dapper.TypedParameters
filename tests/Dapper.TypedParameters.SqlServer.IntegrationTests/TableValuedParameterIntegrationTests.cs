using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.IntegrationTests;

[Collection(SqlServerIntegrationCollectionNames.Default)]
public sealed class TableValuedParameterIntegrationTests
{
    private readonly SqlServerContainerFixture fixture;

    public TableValuedParameterIntegrationTests(SqlServerContainerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task QueryAsync_counts_single_row_from_tvp()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        await ExecuteWithTableTypeAsync(
            connection,
            "Id int NOT NULL, Name nvarchar(50) NOT NULL",
            async typeName =>
            {
                using var table = CreateIdNameTable();
                table.Rows.Add(1, "First");

                int count = await connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM @Items;",
                    new { Items = SqlParam.TableValued(typeName, table) });

                Assert.Equal(1, count);
            });
    }

    [Fact]
    public async Task QueryAsync_aggregates_multiple_rows_from_tvp()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        await ExecuteWithTableTypeAsync(
            connection,
            "Id int NOT NULL, Quantity int NOT NULL",
            async typeName =>
            {
                using var table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Quantity", typeof(int));
                table.Rows.Add(1, 2);
                table.Rows.Add(2, 3);
                table.Rows.Add(3, 5);

                AggregateResult result = await connection.QuerySingleAsync<AggregateResult>(
                    """
                    SELECT
                        COUNT(*) AS Count,
                        SUM(Quantity) AS Total
                    FROM @Items;
                    """,
                    new { Items = SqlParam.TableValued(typeName, table) });

                Assert.Equal(3, result.Count);
                Assert.Equal(10, result.Total);
            });
    }

    [Fact]
    public async Task QueryAsync_accepts_empty_tvp()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        await ExecuteWithTableTypeAsync(
            connection,
            "Id int NOT NULL, Name nvarchar(50) NOT NULL",
            async typeName =>
            {
                using var table = CreateIdNameTable();

                int count = await connection.QuerySingleAsync<int>(
                    "SELECT COUNT(*) FROM @Items;",
                    new { Items = SqlParam.TableValued(typeName, table) });

                Assert.Equal(0, count);
            });
    }

    [Fact]
    public async Task QueryAsync_reads_multiple_columns_and_column_types_from_tvp()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        await ExecuteWithTableTypeAsync(
            connection,
            """
            Id int NOT NULL,
            Name nvarchar(50) NULL,
            ExternalId uniqueidentifier NOT NULL,
            Amount decimal(18, 2) NOT NULL,
            IsActive bit NOT NULL,
            OccurredAt datetime2(7) NOT NULL,
            Payload varbinary(4) NULL
            """,
            async typeName =>
            {
                var id = Guid.Parse("be1ef8e2-35ce-4e41-984d-1ddff3615d75");
                var occurredAt = new DateTime(2026, 8, 10, 9, 30, 0);
                byte[] payload = [0x01, 0x02, 0x03];
                using var table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("ExternalId", typeof(Guid));
                table.Columns.Add("Amount", typeof(decimal));
                table.Columns.Add("IsActive", typeof(bool));
                table.Columns.Add("OccurredAt", typeof(DateTime));
                table.Columns.Add("Payload", typeof(byte[]));
                table.Rows.Add(7, DBNull.Value, id, 123.45M, true, occurredAt, payload);

                MixedColumnResult result = await connection.QuerySingleAsync<MixedColumnResult>(
                    """
                    SELECT
                        Id,
                        Name,
                        ExternalId,
                        Amount,
                        IsActive,
                        OccurredAt,
                        CONVERT(int, DATALENGTH(Payload)) AS PayloadLength
                    FROM @Items;
                    """,
                    new { Items = SqlParam.TableValued(typeName, table) });

                Assert.Equal(7, result.Id);
                Assert.Null(result.Name);
                Assert.Equal(id, result.ExternalId);
                Assert.Equal(123.45M, result.Amount);
                Assert.True(result.IsActive);
                Assert.Equal(occurredAt, result.OccurredAt);
                Assert.Equal(payload.Length, result.PayloadLength);
            });
    }

    [Fact]
    public async Task ExecuteAsync_inserts_tvp_rows_into_table()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        await ExecuteWithTableTypeAsync(
            connection,
            "Id int NOT NULL, Name nvarchar(50) NOT NULL",
            async typeName =>
            {
                await connection.ExecuteAsync(
                    "CREATE TABLE #InsertedItems (Id int NOT NULL, Name nvarchar(50) NOT NULL);");

                try
                {
                    using var table = CreateIdNameTable();
                    table.Rows.Add(1, "First");
                    table.Rows.Add(2, "Second");

                    int affectedRows = await connection.ExecuteAsync(
                        """
                        INSERT INTO #InsertedItems (Id, Name)
                        SELECT Id, Name FROM @Items;
                        """,
                        new { Items = SqlParam.TableValued(typeName, table) });

                    string name = await connection.QuerySingleAsync<string>(
                        "SELECT Name FROM #InsertedItems WHERE Id = 2;");

                    Assert.Equal(2, affectedRows);
                    Assert.Equal("Second", name);
                }
                finally
                {
                    await connection.ExecuteAsync("DROP TABLE IF EXISTS #InsertedItems;");
                }
            });
    }

    [Fact]
    public async Task ExecuteAsync_uses_tvp_in_stored_procedure()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        await ExecuteWithTableTypeAsync(
            connection,
            "Id int NOT NULL, Quantity int NOT NULL",
            async typeName =>
            {
                var procedureName = CreateObjectName("Procedure");
                var fullProcedureName = $"dbo.{procedureName}";
                var quotedProcedureName = QuoteSchemaObject(procedureName);
                await connection.ExecuteAsync(
                    $"""
                    CREATE PROCEDURE {quotedProcedureName}
                        @Items {typeName} READONLY
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT SUM(Quantity) FROM @Items;
                    END
                    """);

                try
                {
                    using var table = new DataTable();
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Quantity", typeof(int));
                    table.Rows.Add(1, 4);
                    table.Rows.Add(2, 6);

                    int total = await connection.QuerySingleAsync<int>(
                        fullProcedureName,
                        new { Items = SqlParam.TableValued(typeName, table) },
                        commandType: CommandType.StoredProcedure);

                    Assert.Equal(10, total);
                }
                finally
                {
                    await connection.ExecuteAsync(
                        $"DROP PROCEDURE IF EXISTS {quotedProcedureName};");
                }
            });
    }

    private static async Task ExecuteWithTableTypeAsync(
        SqlConnection connection,
        string columns,
        Func<string, Task> executeAsync)
    {
        var typeName = CreateObjectName("Type");
        var fullTypeName = $"dbo.{typeName}";
        var quotedTypeName = QuoteSchemaObject(typeName);

        await connection.ExecuteAsync(
            $"CREATE TYPE {quotedTypeName} AS TABLE ({columns});");

        try
        {
            await executeAsync(fullTypeName);
        }
        finally
        {
            await connection.ExecuteAsync($"DROP TYPE IF EXISTS {quotedTypeName};");
        }
    }

    private static DataTable CreateIdNameTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        return table;
    }

    private static string CreateObjectName(string suffix) =>
        $"DtpTvp_{suffix}_{Guid.NewGuid():N}";

    private static string QuoteSchemaObject(string objectName) =>
        $"[dbo].[{objectName}]";

    private sealed class AggregateResult
    {
        public int Count { get; set; }

        public int Total { get; set; }
    }

    private sealed class MixedColumnResult
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public Guid ExternalId { get; set; }

        public decimal Amount { get; set; }

        public bool IsActive { get; set; }

        public DateTime OccurredAt { get; set; }

        public int PayloadLength { get; set; }
    }
}
