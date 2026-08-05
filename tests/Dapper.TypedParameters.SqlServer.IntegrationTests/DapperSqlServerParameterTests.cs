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

    [Fact]
    public async Task QueryAsync_uses_numeric_parameters_with_declared_sql_types()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NumericBaseTypes result = await connection.QuerySingleAsync<NumericBaseTypes>(
            """
            SELECT
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Bit, 'BaseType')) AS Bit,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@TinyInt, 'BaseType')) AS TinyInt,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@SmallInt, 'BaseType')) AS SmallInt,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Int, 'BaseType')) AS Int,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@BigInt, 'BaseType')) AS BigInt,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Real, 'BaseType')) AS Real,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Float, 'BaseType')) AS Float,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Money, 'BaseType')) AS Money,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@SmallMoney, 'BaseType')) AS SmallMoney;
            """,
            new
            {
                Bit = SqlParam.Bit(true),
                TinyInt = SqlParam.TinyInt(byte.MaxValue),
                SmallInt = SqlParam.SmallInt(short.MinValue),
                Int = SqlParam.Int(int.MinValue),
                BigInt = SqlParam.BigInt(long.MinValue),
                Real = SqlParam.Real(-12.5F),
                Float = SqlParam.Float(12.5D),
                Money = SqlParam.Money(-123.45M),
                SmallMoney = SqlParam.SmallMoney(123.45M)
            });

        Assert.Equal("bit", result.Bit);
        Assert.Equal("tinyint", result.TinyInt);
        Assert.Equal("smallint", result.SmallInt);
        Assert.Equal("int", result.Int);
        Assert.Equal("bigint", result.BigInt);
        Assert.Equal("real", result.Real);
        Assert.Equal("float", result.Float);
        Assert.Equal("money", result.Money);
        Assert.Equal("smallmoney", result.SmallMoney);
    }

    [Fact]
    public async Task QueryAsync_round_trips_numeric_values()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NumericRoundTripResult result = await connection.QuerySingleAsync<NumericRoundTripResult>(
            """
            SELECT
                CAST(@Bit AS bit) AS Bit,
                CAST(@TinyInt AS tinyint) AS TinyInt,
                CAST(@SmallInt AS smallint) AS SmallInt,
                CAST(@Int AS int) AS Int,
                CAST(@BigInt AS bigint) AS BigInt,
                CAST(@Real AS real) AS Real,
                CAST(@Float AS float) AS Float,
                CAST(@Money AS money) AS Money,
                CAST(@SmallMoney AS smallmoney) AS SmallMoney;
            """,
            new
            {
                Bit = SqlParam.Bit(true),
                TinyInt = SqlParam.TinyInt(byte.MaxValue),
                SmallInt = SqlParam.SmallInt(short.MinValue),
                Int = SqlParam.Int(int.MaxValue),
                BigInt = SqlParam.BigInt(long.MinValue),
                Real = SqlParam.Real(-12.5F),
                Float = SqlParam.Float(12.5D),
                Money = SqlParam.Money(-123.45M),
                SmallMoney = SqlParam.SmallMoney(123.45M)
            });

        Assert.True(result.Bit);
        Assert.Equal(byte.MaxValue, result.TinyInt);
        Assert.Equal(short.MinValue, result.SmallInt);
        Assert.Equal(int.MaxValue, result.Int);
        Assert.Equal(long.MinValue, result.BigInt);
        Assert.Equal(-12.5F, result.Real);
        Assert.Equal(12.5D, result.Float);
        Assert.Equal(-123.45M, result.Money);
        Assert.Equal(123.45M, result.SmallMoney);
    }

    [Fact]
    public async Task QueryAsync_uses_decimal_18_2_with_declared_precision_and_scale()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        DecimalMetadata result = await connection.QuerySingleAsync<DecimalMetadata>(
            """
            SELECT
                CAST(@Amount AS decimal(18, 2)) AS Value,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Amount, 'BaseType')) AS BaseType,
                CONVERT(int, SQL_VARIANT_PROPERTY(@Amount, 'Precision')) AS Precision,
                CONVERT(int, SQL_VARIANT_PROPERTY(@Amount, 'Scale')) AS Scale;
            """,
            new
            {
                Amount = SqlParam.Decimal(123.45M, 18, 2)
            });

        Assert.Equal(123.45M, result.Value);
        Assert.Equal("decimal", result.BaseType);
        Assert.Equal(18, result.Precision);
        Assert.Equal(2, result.Scale);
    }

    [Fact]
    public async Task QueryAsync_uses_decimal_38_18_with_declared_precision_and_scale()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        const decimal amount = 1234567890.123456789012345678M;

        DecimalMetadata result = await connection.QuerySingleAsync<DecimalMetadata>(
            """
            SELECT
                CAST(@Amount AS decimal(38, 18)) AS Value,
                CONVERT(nvarchar(128), SQL_VARIANT_PROPERTY(@Amount, 'BaseType')) AS BaseType,
                CONVERT(int, SQL_VARIANT_PROPERTY(@Amount, 'Precision')) AS Precision,
                CONVERT(int, SQL_VARIANT_PROPERTY(@Amount, 'Scale')) AS Scale;
            """,
            new
            {
                Amount = SqlParam.Decimal(amount, 38, 18)
            });

        Assert.Equal(amount, result.Value);
        Assert.Equal("decimal", result.BaseType);
        Assert.Equal(38, result.Precision);
        Assert.Equal(18, result.Scale);
    }

    [Fact]
    public async Task QueryAsync_sends_numeric_null_value_as_database_null()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        bool isNull = await connection.QuerySingleAsync<bool>(
            "SELECT CONVERT(bit, CASE WHEN @Amount IS NULL THEN 1 ELSE 0 END);",
            new
            {
                Amount = SqlParam.Decimal(null, 18, 2)
            });

        Assert.True(isNull);
    }

    [Fact]
    public async Task ExecuteAsync_inserts_and_selects_numeric_values_using_where_parameter()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            CREATE TABLE #Numbers
            (
                Id int NOT NULL,
                IsActive bit NOT NULL,
                TinyValue tinyint NOT NULL,
                SmallValue smallint NOT NULL,
                IntValue int NOT NULL,
                BigValue bigint NOT NULL,
                RealValue real NOT NULL,
                FloatValue float NOT NULL,
                Amount decimal(18, 2) NOT NULL,
                MoneyValue money NOT NULL,
                SmallMoneyValue smallmoney NOT NULL
            );
            """);

        try
        {
            int affectedRows = await connection.ExecuteAsync(
                """
                INSERT INTO #Numbers
                (
                    Id,
                    IsActive,
                    TinyValue,
                    SmallValue,
                    IntValue,
                    BigValue,
                    RealValue,
                    FloatValue,
                    Amount,
                    MoneyValue,
                    SmallMoneyValue
                )
                VALUES
                (
                    @Id,
                    @IsActive,
                    @TinyValue,
                    @SmallValue,
                    @IntValue,
                    @BigValue,
                    @RealValue,
                    @FloatValue,
                    @Amount,
                    @MoneyValue,
                    @SmallMoneyValue
                );
                """,
                new
                {
                    Id = SqlParam.Int(1),
                    IsActive = SqlParam.Bit(true),
                    TinyValue = SqlParam.TinyInt(byte.MaxValue),
                    SmallValue = SqlParam.SmallInt(short.MinValue),
                    IntValue = SqlParam.Int(int.MaxValue),
                    BigValue = SqlParam.BigInt(long.MinValue),
                    RealValue = SqlParam.Real(-12.5F),
                    FloatValue = SqlParam.Float(12.5D),
                    Amount = SqlParam.Decimal(-123.45M, 18, 2),
                    MoneyValue = SqlParam.Money(-922337203685477.5808M),
                    SmallMoneyValue = SqlParam.SmallMoney(-214748.3648M)
                });

            NumericTableRow result = await connection.QuerySingleAsync<NumericTableRow>(
                """
                SELECT
                    Id,
                    IsActive,
                    TinyValue,
                    SmallValue,
                    IntValue,
                    BigValue,
                    RealValue,
                    FloatValue,
                    Amount,
                    MoneyValue,
                    SmallMoneyValue
                FROM #Numbers
                WHERE Id = @Id;
                """,
                new
                {
                    Id = SqlParam.Int(1)
                });

            Assert.Equal(1, affectedRows);
            Assert.Equal(1, result.Id);
            Assert.True(result.IsActive);
            Assert.Equal(byte.MaxValue, result.TinyValue);
            Assert.Equal(short.MinValue, result.SmallValue);
            Assert.Equal(int.MaxValue, result.IntValue);
            Assert.Equal(long.MinValue, result.BigValue);
            Assert.Equal(-12.5F, result.RealValue);
            Assert.Equal(12.5D, result.FloatValue);
            Assert.Equal(-123.45M, result.Amount);
            Assert.Equal(-922337203685477.5808M, result.MoneyValue);
            Assert.Equal(-214748.3648M, result.SmallMoneyValue);
        }
        finally
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS #Numbers;");
        }
    }

    private sealed class NumericBaseTypes
    {
        public string Bit { get; set; } = string.Empty;

        public string TinyInt { get; set; } = string.Empty;

        public string SmallInt { get; set; } = string.Empty;

        public string Int { get; set; } = string.Empty;

        public string BigInt { get; set; } = string.Empty;

        public string Real { get; set; } = string.Empty;

        public string Float { get; set; } = string.Empty;

        public string Money { get; set; } = string.Empty;

        public string SmallMoney { get; set; } = string.Empty;
    }

    private sealed class NumericRoundTripResult
    {
        public bool Bit { get; set; }

        public byte TinyInt { get; set; }

        public short SmallInt { get; set; }

        public int Int { get; set; }

        public long BigInt { get; set; }

        public float Real { get; set; }

        public double Float { get; set; }

        public decimal Money { get; set; }

        public decimal SmallMoney { get; set; }
    }

    private sealed class DecimalMetadata
    {
        public decimal Value { get; set; }

        public string BaseType { get; set; } = string.Empty;

        public int Precision { get; set; }

        public int Scale { get; set; }
    }

    private sealed class NumericTableRow
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public byte TinyValue { get; set; }

        public short SmallValue { get; set; }

        public int IntValue { get; set; }

        public long BigValue { get; set; }

        public float RealValue { get; set; }

        public double FloatValue { get; set; }

        public decimal Amount { get; set; }

        public decimal MoneyValue { get; set; }

        public decimal SmallMoneyValue { get; set; }
    }
}
