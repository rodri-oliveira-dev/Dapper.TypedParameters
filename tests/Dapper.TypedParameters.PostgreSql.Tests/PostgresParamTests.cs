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
            { "Money", NpgsqlDbType.Money },
            { "Uuid", NpgsqlDbType.Uuid },
            { "Bytea", NpgsqlDbType.Bytea },
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
                "BigInt",
                "Boolean",
                "Bytea",
                "Double",
                "Integer",
                "Money",
                "Real",
                "SmallInt",
                "Text",
                "Uuid",
            },
            factoryNames);
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
            "Money" => PostgresParam.Money(-12.34M),
            "Uuid" => PostgresParam.Uuid(Guid.Parse("f0da086a-cf8d-4682-8a55-e96017890d2b")),
            "Bytea" => PostgresParam.Bytea([0x01, 0x02]),
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
            "Money" => PostgresParam.Money(null),
            "Uuid" => PostgresParam.Uuid(null),
            "Bytea" => PostgresParam.Bytea(null),
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
            "Money" => -12.34M,
            "Uuid" => Guid.Parse("f0da086a-cf8d-4682-8a55-e96017890d2b"),
            "Bytea" => new byte[] { 0x01, 0x02 },
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };
}
