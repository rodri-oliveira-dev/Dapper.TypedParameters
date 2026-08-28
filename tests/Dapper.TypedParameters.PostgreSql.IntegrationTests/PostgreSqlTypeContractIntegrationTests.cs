using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using Xunit;

namespace Dapper.TypedParameters.PostgreSql.IntegrationTests;

[Collection(PostgreSqlIntegrationCollectionNames.Default)]
public sealed class PostgreSqlTypeContractIntegrationTests
{
    private readonly PostgreSqlContainerFixture fixture;

    public PostgreSqlTypeContractIntegrationTests(PostgreSqlContainerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData("ab", 3, "ab", 2)]
    [InlineData("abc", 3, "abc", 3)]
    [InlineData("abcd", 3, "abc", 3)]
    public async Task Varchar_size_truncates_sent_value_but_does_not_create_backend_typmod(
        string value,
        int size,
        string expectedText,
        int expectedLength)
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        TextProbe result = await connection.QuerySingleAsync<TextProbe>(
            TextProbeSql,
            new
            {
                Value = new RawNpgsqlParameter(
                    value,
                    NpgsqlDbType.Varchar,
                    size: size)
            });

        Assert.Equal("character varying", result.TypeName);
        Assert.Equal(expectedText, result.TextValue);
        Assert.Equal(expectedLength, result.TextLength);
    }

    [Theory]
    [InlineData("ab", 3, "ab", 2)]
    [InlineData("abc", 3, "abc", 3)]
    [InlineData("abcd", 3, "abc", 3)]
    public async Task Char_size_truncates_sent_value_but_does_not_create_backend_typmod(
        string value,
        int size,
        string expectedText,
        int expectedLength)
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        TextProbe result = await connection.QuerySingleAsync<TextProbe>(
            TextProbeSql,
            new
            {
                Value = new RawNpgsqlParameter(
                    value,
                    NpgsqlDbType.Char,
                    size: size)
            });

        Assert.Equal("character", result.TypeName);
        Assert.Equal(expectedText, result.TextValue);
        Assert.Equal(expectedLength, result.TextLength);
    }

    [Fact]
    public async Task Varchar_null_preserves_backend_type()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NullProbe result = await connection.QuerySingleAsync<NullProbe>(
            NullProbeSql,
            new
            {
                Value = new RawNpgsqlParameter(
                    null,
                    NpgsqlDbType.Varchar,
                    size: 3)
            });

        Assert.Equal("character varying", result.TypeName);
        Assert.True(result.IsNull);
    }

    [Fact]
    public async Task Char_null_preserves_backend_type()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NullProbe result = await connection.QuerySingleAsync<NullProbe>(
            NullProbeSql,
            new
            {
                Value = new RawNpgsqlParameter(
                    null,
                    NpgsqlDbType.Char,
                    size: 3)
            });

        Assert.Equal("character", result.TypeName);
        Assert.True(result.IsNull);
    }

    [Fact]
    public async Task Numeric_precision_and_scale_are_client_metadata_not_backend_typmod()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NumericProbe result = await connection.QuerySingleAsync<NumericProbe>(
            """
            SELECT
                pg_typeof(@Value)::text AS TypeName,
                @Value AS Value,
                @Value::text AS TextValue;
            """,
            new
            {
                Value = new RawNpgsqlParameter(
                    12345.6789M,
                    NpgsqlDbType.Numeric,
                    precision: 5,
                    scale: 2)
            });

        Assert.Equal("numeric", result.TypeName);
        Assert.Equal(12345.6789M, result.Value);
        Assert.Equal("12345.6789", result.TextValue);
    }

    [Theory]
    [InlineData("VarChar", "character varying", "typed")]
    [InlineData("Char", "character", "typed")]
    [InlineData("Json", "json", "{\"name\":\"typed\"}")]
    [InlineData("Jsonb", "jsonb", "{\"name\": \"typed\"}")]
    public async Task Text_and_json_factories_send_declared_backend_type(
        string factoryName,
        string expectedType,
        string expectedText)
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        TextProbe result = await connection.QuerySingleAsync<TextProbe>(
            TextProbeSql,
            new
            {
                Value = CreateTextLikeParameter(factoryName)
            });

        Assert.Equal(expectedType, result.TypeName);
        Assert.Equal(expectedText, result.TextValue);
    }

    [Theory]
    [InlineData("Json", "json")]
    [InlineData("Jsonb", "jsonb")]
    public async Task Json_factories_send_null_with_declared_backend_type(
        string factoryName,
        string expectedType)
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NullProbe result = await connection.QuerySingleAsync<NullProbe>(
            NullProbeSql,
            new
            {
                Value = CreateNullJsonParameter(factoryName)
            });

        Assert.Equal(expectedType, result.TypeName);
        Assert.True(result.IsNull);
    }

    [Fact]
    public async Task Json_round_trips_text_input_without_serialization_policy()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string name = await connection.QuerySingleAsync<string>(
            "SELECT @Value::json ->> 'name';",
            new
            {
                Value = PostgresParam.Json("{\"name\":\"typed\"}")
            });

        Assert.Equal("typed", name);
    }

    [Fact]
    public async Task Jsonb_round_trips_text_input_without_serialization_policy()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string name = await connection.QuerySingleAsync<string>(
            "SELECT @Value::jsonb ->> 'name';",
            new
            {
                Value = PostgresParam.Jsonb("{\"name\":\"typed\"}")
            });

        Assert.Equal("typed", name);
    }

    [Fact]
    public async Task Numeric_factory_sends_unconstrained_numeric_type()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NumericProbe result = await connection.QuerySingleAsync<NumericProbe>(
            """
            SELECT
                pg_typeof(@Value)::text AS TypeName,
                @Value AS Value,
                @Value::text AS TextValue;
            """,
            new
            {
                Value = PostgresParam.Numeric(12345.6789M)
            });

        Assert.Equal("numeric", result.TypeName);
        Assert.Equal(12345.6789M, result.Value);
        Assert.Equal("12345.6789", result.TextValue);
    }

    [Theory]
    [InlineData("Date", "date")]
    [InlineData("Time", "time without time zone")]
    [InlineData("Timestamp", "timestamp without time zone")]
    [InlineData("TimestampTz", "timestamp with time zone")]
    [InlineData("Interval", "interval")]
    public async Task Temporal_factories_send_declared_backend_type(
        string factoryName,
        string expectedType)
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string typeName = await connection.QuerySingleAsync<string>(
            "SELECT pg_typeof(@Value)::text;",
            new
            {
                Value = CreateTemporalParameter(factoryName)
            });

        Assert.Equal(expectedType, typeName);
    }

    [Fact]
    public async Task Date_round_trips_dateonly_value()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string value = await connection.QuerySingleAsync<string>(
            "SELECT @Value::text;",
            new
            {
                Value = PostgresParam.Date(new DateOnly(2026, 8, 28))
            });

        Assert.Equal("2026-08-28", value);
    }

    [Fact]
    public async Task Time_round_trips_timeonly_value()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        string value = await connection.QuerySingleAsync<string>(
            "SELECT @Value::text;",
            new
            {
                Value = PostgresParam.Time(new TimeOnly(13, 45, 12, 123))
            });

        Assert.Equal("13:45:12.123", value);
    }

    [Fact]
    public async Task Timestamp_round_trips_unspecified_wall_clock_value()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        DateTime wallClock = new(2026, 8, 28, 13, 45, 12, DateTimeKind.Unspecified);

        DateTime value = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value;",
            new
            {
                Value = PostgresParam.Timestamp(wallClock)
            });

        Assert.Equal(wallClock, value);
        Assert.Equal(DateTimeKind.Unspecified, value.Kind);
    }

    [Fact]
    public async Task TimestampTz_round_trips_utc_instant()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();
        DateTime instant = new(2026, 8, 28, 16, 45, 12, DateTimeKind.Utc);

        DateTime value = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value;",
            new
            {
                Value = PostgresParam.TimestampTz(instant)
            });

        Assert.Equal(instant, value);
        Assert.Equal(DateTimeKind.Utc, value.Kind);
    }

    [Fact]
    public async Task Interval_round_trips_timespan_value()
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        TimeSpan value = await connection.QuerySingleAsync<TimeSpan>(
            "SELECT @Value;",
            new
            {
                Value = PostgresParam.Interval(TimeSpan.FromDays(2) + TimeSpan.FromMinutes(3))
            });

        Assert.Equal(TimeSpan.FromDays(2) + TimeSpan.FromMinutes(3), value);
    }

    [Theory]
    [InlineData("Date", "date")]
    [InlineData("Time", "time without time zone")]
    [InlineData("Timestamp", "timestamp without time zone")]
    [InlineData("TimestampTz", "timestamp with time zone")]
    [InlineData("Interval", "interval")]
    public async Task Temporal_factories_send_null_with_declared_backend_type(
        string factoryName,
        string expectedType)
    {
        await using var connection = fixture.CreateConnection();
        await connection.OpenAsync();

        NullProbe result = await connection.QuerySingleAsync<NullProbe>(
            NullProbeSql,
            new
            {
                Value = CreateNullTemporalParameter(factoryName)
            });

        Assert.Equal(expectedType, result.TypeName);
        Assert.True(result.IsNull);
    }

    private const string TextProbeSql =
        """
        SELECT
            pg_typeof(@Value)::text AS TypeName,
            @Value::text AS TextValue,
            length(@Value::text) AS TextLength;
        """;

    private const string NullProbeSql =
        """
        SELECT
            pg_typeof(@Value)::text AS TypeName,
            @Value IS NULL AS IsNull;
        """;

    private static TypedPostgresParameter CreateTextLikeParameter(string factoryName) =>
        factoryName switch
        {
            "VarChar" => PostgresParam.VarChar("typed"),
            "Char" => PostgresParam.Char("typed"),
            "Json" => PostgresParam.Json("{\"name\":\"typed\"}"),
            "Jsonb" => PostgresParam.Jsonb("{\"name\":\"typed\"}"),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private static TypedPostgresParameter CreateNullJsonParameter(string factoryName) =>
        factoryName switch
        {
            "Json" => PostgresParam.Json(null),
            "Jsonb" => PostgresParam.Jsonb(null),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private static TypedPostgresParameter CreateTemporalParameter(string factoryName) =>
        factoryName switch
        {
            "Date" => PostgresParam.Date(new DateOnly(2026, 8, 28)),
            "Time" => PostgresParam.Time(new TimeOnly(13, 45, 12)),
            "Timestamp" => PostgresParam.Timestamp(new DateTime(
                2026,
                8,
                28,
                13,
                45,
                12,
                DateTimeKind.Unspecified)),
            "TimestampTz" => PostgresParam.TimestampTz(new DateTime(
                2026,
                8,
                28,
                16,
                45,
                12,
                DateTimeKind.Utc)),
            "Interval" => PostgresParam.Interval(TimeSpan.FromHours(25)),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private static TypedPostgresParameter CreateNullTemporalParameter(string factoryName) =>
        factoryName switch
        {
            "Date" => PostgresParam.Date(null),
            "Time" => PostgresParam.Time(null),
            "Timestamp" => PostgresParam.Timestamp(null),
            "TimestampTz" => PostgresParam.TimestampTz(null),
            "Interval" => PostgresParam.Interval(null),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private sealed class RawNpgsqlParameter : SqlMapper.ICustomQueryParameter
    {
        private readonly object? value;
        private readonly NpgsqlDbType npgsqlDbType;
        private readonly int? size;
        private readonly byte? precision;
        private readonly byte? scale;

        public RawNpgsqlParameter(
            object? value,
            NpgsqlDbType npgsqlDbType,
            int? size = null,
            byte? precision = null,
            byte? scale = null)
        {
            this.value = value;
            this.npgsqlDbType = npgsqlDbType;
            this.size = size;
            this.precision = precision;
            this.scale = scale;
        }

        public void AddParameter(IDbCommand command, string name)
        {
            if (command is not NpgsqlCommand npgsqlCommand)
            {
                throw new NotSupportedException();
            }

            var parameter = npgsqlCommand.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.NpgsqlDbType = npgsqlDbType;

            if (size.HasValue)
            {
                parameter.Size = size.GetValueOrDefault();
            }

            if (precision.HasValue)
            {
                parameter.Precision = precision.GetValueOrDefault();
            }

            if (scale.HasValue)
            {
                parameter.Scale = scale.GetValueOrDefault();
            }

            npgsqlCommand.Parameters.Add(parameter);
        }
    }

    private sealed class TextProbe
    {
        public string TypeName { get; set; } = string.Empty;

        public string TextValue { get; set; } = string.Empty;

        public int TextLength { get; set; }
    }

    private sealed class NullProbe
    {
        public string TypeName { get; set; } = string.Empty;

        public bool IsNull { get; set; }
    }

    private sealed class NumericProbe
    {
        public string TypeName { get; set; } = string.Empty;

        public decimal Value { get; set; }

        public string TextValue { get; set; } = string.Empty;
    }
}
