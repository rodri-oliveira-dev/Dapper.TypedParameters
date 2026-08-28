using NpgsqlTypes;
using Xunit;

namespace Dapper.TypedParameters.PostgreSql.Tests;

public sealed class PostgresParamTests
{
    public static TheoryData<string, NpgsqlDbType> ScalarFactories =>
        new()
        {
            { "Text", NpgsqlDbType.Text },
            { "Boolean", NpgsqlDbType.Boolean },
            { "SmallInt", NpgsqlDbType.Smallint },
            { "Integer", NpgsqlDbType.Integer },
            { "BigInt", NpgsqlDbType.Bigint },
            { "Real", NpgsqlDbType.Real },
            { "Double", NpgsqlDbType.Double },
            { "Numeric", NpgsqlDbType.Numeric },
            { "Money", NpgsqlDbType.Money },
            { "Uuid", NpgsqlDbType.Uuid },
            { "Bytea", NpgsqlDbType.Bytea },
            { "VarChar", NpgsqlDbType.Varchar },
            { "Char", NpgsqlDbType.Char },
            { "Json", NpgsqlDbType.Json },
            { "Jsonb", NpgsqlDbType.Jsonb },
            { "Date", NpgsqlDbType.Date },
            { "Time", NpgsqlDbType.Time },
            { "Timestamp", NpgsqlDbType.Timestamp },
            { "TimestampTz", NpgsqlDbType.TimestampTz },
            { "Interval", NpgsqlDbType.Interval },
        };

    public static TheoryData<NpgsqlDbType> InvalidArrayElementTypes =>
        new()
        {
            NpgsqlDbType.Array,
            NpgsqlDbType.Array | NpgsqlDbType.Integer,
            NpgsqlDbType.Range,
            NpgsqlDbType.IntegerRange,
            NpgsqlDbType.Multirange,
            NpgsqlDbType.IntegerMultirange,
        };

    public static TheoryData<NpgsqlDbType> UnsupportedArrayElementTypes =>
        new()
        {
            NpgsqlDbType.Unknown,
            NpgsqlDbType.Inet,
            NpgsqlDbType.Hstore,
        };

    [Theory]
    [MemberData(nameof(ScalarFactories))]
    public void Scalar_factories_create_expected_contract(
        string factoryName,
        NpgsqlDbType expectedNpgsqlDbType)
    {
        var parameter = CreateNonNullParameter(factoryName);
        object expectedValue = GetExpectedValue(factoryName);

        Assert.IsType<TypedPostgresParameter>(parameter);
        Assert.Equal(expectedValue, parameter.Value);
        Assert.Equal(expectedNpgsqlDbType, parameter.NpgsqlDbType);
    }

    [Theory]
    [MemberData(nameof(ScalarFactories))]
    public void Scalar_factories_accept_null_values(
        string factoryName,
        NpgsqlDbType expectedNpgsqlDbType)
    {
        var parameter = CreateNullParameter(factoryName);

        Assert.IsType<TypedPostgresParameter>(parameter);
        Assert.Null(parameter.Value);
        Assert.Equal(expectedNpgsqlDbType, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Bytea_preserves_empty_array()
    {
        byte[] value = [];

        var parameter = PostgresParam.Bytea(value);

        Assert.Same(value, parameter.Value);
        Assert.Empty((byte[])parameter.Value!);
        Assert.Equal(NpgsqlDbType.Bytea, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Uuid_accepts_empty_guid()
    {
        var parameter = PostgresParam.Uuid(Guid.Empty);

        Assert.Equal(Guid.Empty, parameter.Value);
        Assert.Equal(NpgsqlDbType.Uuid, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Array_accepts_array_value_without_copying()
    {
        int[] value = [1, 2, 3];

        var parameter = PostgresParam.Array(value, NpgsqlDbType.Integer);

        Assert.Same(value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Integer, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Array_accepts_list_value_without_copying()
    {
        List<Guid> value =
        [
            Guid.Parse("f0da086a-cf8d-4682-8a55-e96017890d2b"),
            Guid.Parse("50b71c82-12f8-4f0b-8d8c-f03017d3a48c"),
        ];

        var parameter = PostgresParam.Array(value, NpgsqlDbType.Uuid);

        Assert.Same(value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Uuid, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Array_preserves_empty_array()
    {
        string[] value = [];

        var parameter = PostgresParam.Array(value, NpgsqlDbType.Text);

        Assert.Same(value, parameter.Value);
        Assert.Empty(Assert.IsAssignableFrom<IList<string>>(parameter.Value));
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Text, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Array_accepts_null_value()
    {
        var parameter = PostgresParam.Array<int>(null, NpgsqlDbType.Integer);

        Assert.Null(parameter.Value);
        Assert.Equal(NpgsqlDbType.Array | NpgsqlDbType.Integer, parameter.NpgsqlDbType);
    }

    [Theory]
    [MemberData(nameof(InvalidArrayElementTypes))]
    public void Array_rejects_element_types_with_array_range_or_multirange_semantics(
        NpgsqlDbType elementType)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => PostgresParam.Array<int>([], elementType));

        Assert.Equal("elementType", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(UnsupportedArrayElementTypes))]
    public void Array_rejects_element_types_outside_v1_scalar_contract(
        NpgsqlDbType elementType)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => PostgresParam.Array<string>([], elementType));

        Assert.Equal("elementType", exception.ParamName);
    }

    [Fact]
    public void PostgresParam_public_factories_are_explicit()
    {
        string[] factoryNames = typeof(PostgresParam)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(PostgresParam))
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "Array",
                "BigInt",
                "Boolean",
                "Bytea",
                "Char",
                "Date",
                "Double",
                "Integer",
                "Interval",
                "Json",
                "Jsonb",
                "Money",
                "Numeric",
                "Real",
                "SmallInt",
                "Text",
                "Time",
                "Timestamp",
                "TimestampTz",
                "Uuid",
                "VarChar",
            },
            factoryNames);
    }

    [Fact]
    public void Timestamp_rejects_utc_datetime()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => PostgresParam.Timestamp(new DateTime(2026, 8, 28, 13, 45, 12, DateTimeKind.Utc)));

        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Timestamp_accepts_local_or_unspecified_wall_clock_datetime(DateTimeKind kind)
    {
        DateTime value = new(2026, 8, 28, 13, 45, 12, kind);

        var parameter = PostgresParam.Timestamp(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(NpgsqlDbType.Timestamp, parameter.NpgsqlDbType);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void TimestampTz_rejects_non_utc_datetime(DateTimeKind kind)
    {
        DateTime value = new(2026, 8, 28, 13, 45, 12, kind);

        var exception = Assert.Throws<ArgumentException>(
            () => PostgresParam.TimestampTz(value));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void TimestampTz_accepts_utc_datetime()
    {
        DateTime value = new(2026, 8, 28, 13, 45, 12, DateTimeKind.Utc);

        var parameter = PostgresParam.TimestampTz(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(NpgsqlDbType.TimestampTz, parameter.NpgsqlDbType);
    }

    public static TypedPostgresParameter CreateNonNullParameter(string factoryName) =>
        factoryName switch
        {
            "Text" => PostgresParam.Text("value"),
            "Boolean" => PostgresParam.Boolean(true),
            "SmallInt" => PostgresParam.SmallInt(short.MinValue),
            "Integer" => PostgresParam.Integer(int.MinValue),
            "BigInt" => PostgresParam.BigInt(long.MinValue),
            "Real" => PostgresParam.Real(12.5F),
            "Double" => PostgresParam.Double(12.5D),
            "Numeric" => PostgresParam.Numeric(12345.6789M),
            "Money" => PostgresParam.Money(-12.34M),
            "Uuid" => PostgresParam.Uuid(Guid.Parse("f0da086a-cf8d-4682-8a55-e96017890d2b")),
            "Bytea" => PostgresParam.Bytea([0x01, 0x02]),
            "VarChar" => PostgresParam.VarChar("value"),
            "Char" => PostgresParam.Char("value"),
            "Json" => PostgresParam.Json("{\"name\":\"typed\"}"),
            "Jsonb" => PostgresParam.Jsonb("{\"name\":\"typed\"}"),
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

    private static TypedPostgresParameter CreateNullParameter(string factoryName) =>
        factoryName switch
        {
            "Text" => PostgresParam.Text(null),
            "Boolean" => PostgresParam.Boolean(null),
            "SmallInt" => PostgresParam.SmallInt(null),
            "Integer" => PostgresParam.Integer(null),
            "BigInt" => PostgresParam.BigInt(null),
            "Real" => PostgresParam.Real(null),
            "Double" => PostgresParam.Double(null),
            "Numeric" => PostgresParam.Numeric(null),
            "Money" => PostgresParam.Money(null),
            "Uuid" => PostgresParam.Uuid(null),
            "Bytea" => PostgresParam.Bytea(null),
            "VarChar" => PostgresParam.VarChar(null),
            "Char" => PostgresParam.Char(null),
            "Json" => PostgresParam.Json(null),
            "Jsonb" => PostgresParam.Jsonb(null),
            "Date" => PostgresParam.Date(null),
            "Time" => PostgresParam.Time(null),
            "Timestamp" => PostgresParam.Timestamp(null),
            "TimestampTz" => PostgresParam.TimestampTz(null),
            "Interval" => PostgresParam.Interval(null),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private static object GetExpectedValue(string factoryName) =>
        factoryName switch
        {
            "Text" => "value",
            "Boolean" => true,
            "SmallInt" => short.MinValue,
            "Integer" => int.MinValue,
            "BigInt" => long.MinValue,
            "Real" => 12.5F,
            "Double" => 12.5D,
            "Numeric" => 12345.6789M,
            "Money" => -12.34M,
            "Uuid" => Guid.Parse("f0da086a-cf8d-4682-8a55-e96017890d2b"),
            "Bytea" => new byte[] { 0x01, 0x02 },
            "VarChar" => "value",
            "Char" => "value",
            "Json" => "{\"name\":\"typed\"}",
            "Jsonb" => "{\"name\":\"typed\"}",
            "Date" => new DateOnly(2026, 8, 28),
            "Time" => new TimeOnly(13, 45, 12),
            "Timestamp" => new DateTime(2026, 8, 28, 13, 45, 12, DateTimeKind.Unspecified),
            "TimestampTz" => new DateTime(2026, 8, 28, 16, 45, 12, DateTimeKind.Utc),
            "Interval" => TimeSpan.FromHours(25),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };
}
