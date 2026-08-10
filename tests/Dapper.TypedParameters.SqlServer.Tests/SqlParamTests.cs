using System.Data;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.Tests;

public sealed class SqlParamTests
{
    public static TheoryData<string, SqlDbType> NumericFactories =>
        new()
        {
            { "Bit", SqlDbType.Bit },
            { "TinyInt", SqlDbType.TinyInt },
            { "SmallInt", SqlDbType.SmallInt },
            { "Int", SqlDbType.Int },
            { "BigInt", SqlDbType.BigInt },
            { "Real", SqlDbType.Real },
            { "Float", SqlDbType.Float },
            { "Money", SqlDbType.Money },
            { "SmallMoney", SqlDbType.SmallMoney },
        };

    public static TheoryData<string, SqlDbType> NullNumericFactories =>
        new()
        {
            { "Bit", SqlDbType.Bit },
            { "TinyInt", SqlDbType.TinyInt },
            { "SmallInt", SqlDbType.SmallInt },
            { "Int", SqlDbType.Int },
            { "BigInt", SqlDbType.BigInt },
            { "Real", SqlDbType.Real },
            { "Float", SqlDbType.Float },
            { "Decimal", SqlDbType.Decimal },
            { "Money", SqlDbType.Money },
            { "SmallMoney", SqlDbType.SmallMoney },
        };

    public static TheoryData<string> ScalarFactories =>
        new()
        {
            "VarChar",
            "NVarChar",
            "Char",
            "NChar",
            "VarCharMax",
            "NVarCharMax",
            "Bit",
            "TinyInt",
            "SmallInt",
            "Int",
            "BigInt",
            "Real",
            "Float",
            "Decimal",
            "Money",
            "SmallMoney",
            "UniqueIdentifier",
            "Binary",
            "VarBinary",
            "VarBinaryMax",
            "Date",
            "Time",
            "DateTime",
            "SmallDateTime",
            "DateTime2",
            "DateTimeOffset",
        };

    [Fact]
    public void VarChar_creates_expected_contract()
    {
        var parameter = SqlParam.VarChar("12345678901", 11);

        Assert.Equal("12345678901", parameter.Value);
        Assert.Equal(SqlDbType.VarChar, parameter.SqlDbType);
        Assert.Equal(11, parameter.Size);
    }

    [Fact]
    public void NVarChar_creates_expected_contract()
    {
        var parameter = SqlParam.NVarChar("Rodrigo", 150);

        Assert.Equal("Rodrigo", parameter.Value);
        Assert.Equal(SqlDbType.NVarChar, parameter.SqlDbType);
        Assert.Equal(150, parameter.Size);
    }

    [Fact]
    public void Char_creates_expected_contract()
    {
        var parameter = SqlParam.Char("SP", 2);

        Assert.Equal("SP", parameter.Value);
        Assert.Equal(SqlDbType.Char, parameter.SqlDbType);
        Assert.Equal(2, parameter.Size);
    }

    [Fact]
    public void NChar_creates_expected_contract()
    {
        var parameter = SqlParam.NChar("A", 1);

        Assert.Equal("A", parameter.Value);
        Assert.Equal(SqlDbType.NChar, parameter.SqlDbType);
        Assert.Equal(1, parameter.Size);
    }

    [Fact]
    public void VarCharMax_uses_sql_server_max_size()
    {
        var parameter = SqlParam.VarCharMax("value");

        Assert.Equal(SqlDbType.VarChar, parameter.SqlDbType);
        Assert.Equal(-1, parameter.Size);
    }

    [Fact]
    public void NVarCharMax_uses_sql_server_max_size()
    {
        var parameter = SqlParam.NVarCharMax("value");

        Assert.Equal(SqlDbType.NVarChar, parameter.SqlDbType);
        Assert.Equal(-1, parameter.Size);
    }

    [Theory]
    [MemberData(nameof(ScalarFactories))]
    public void Scalar_factories_default_to_input_direction(
        string factoryName)
    {
        var parameter = CreateScalarParameter(factoryName);

        Assert.NotEmpty(factoryName);
        Assert.Equal(ParameterDirection.Input, parameter.Direction);
    }

    [Theory]
    [MemberData(nameof(ScalarFactories))]
    public void AsOutput_configures_output_direction_for_scalar_parameters(
        string factoryName)
    {
        var parameter = CreateScalarParameter(factoryName);
        var output = parameter.AsOutput();

        Assert.NotEmpty(factoryName);
        Assert.Equal(ParameterDirection.Output, output.Direction);
        Assert.Equal(parameter.Value, output.Value);
        Assert.Equal(parameter.SqlDbType, output.SqlDbType);
        Assert.Equal(parameter.Size, output.Size);
        Assert.Equal(parameter.Precision, output.Precision);
        Assert.Equal(parameter.Scale, output.Scale);
    }

    [Theory]
    [MemberData(nameof(ScalarFactories))]
    public void AsInputOutput_configures_input_output_direction_for_scalar_parameters(
        string factoryName)
    {
        var parameter = CreateScalarParameter(factoryName);
        var inputOutput = parameter.AsInputOutput();

        Assert.NotEmpty(factoryName);
        Assert.Equal(ParameterDirection.InputOutput, inputOutput.Direction);
        Assert.Equal(parameter.Value, inputOutput.Value);
        Assert.Equal(parameter.SqlDbType, inputOutput.SqlDbType);
        Assert.Equal(parameter.Size, inputOutput.Size);
        Assert.Equal(parameter.Precision, inputOutput.Precision);
        Assert.Equal(parameter.Scale, inputOutput.Scale);
    }

    [Fact]
    public void Date_creates_expected_contract()
    {
        var value = new DateOnly(2026, 8, 5);

        var parameter = SqlParam.Date(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(SqlDbType.Date, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void Date_accepts_null()
    {
        var parameter = SqlParam.Date(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.Date, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void Time_uses_default_scale()
    {
        var value = new TimeOnly(12, 34, 56, 789);

        var parameter = SqlParam.Time(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(SqlDbType.Time, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Equal((byte?)7, parameter.Scale);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Time_accepts_scale_boundaries(byte scale)
    {
        var parameter = SqlParam.Time(new TimeOnly(12, 34, 56), scale);

        Assert.Equal((byte?)scale, parameter.Scale);
    }

    [Fact]
    public void DateTime_creates_expected_contract()
    {
        var value = new DateTime(2026, 8, 5, 12, 34, 56, 789, DateTimeKind.Local);

        var parameter = SqlParam.DateTime(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(DateTimeKind.Local, ((DateTime)parameter.Value!).Kind);
        Assert.Equal(SqlDbType.DateTime, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void DateTime_accepts_null()
    {
        var parameter = SqlParam.DateTime(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.DateTime, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void SmallDateTime_creates_expected_contract()
    {
        var value = new DateTime(2026, 8, 5, 12, 34, 56, DateTimeKind.Unspecified);

        var parameter = SqlParam.SmallDateTime(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(DateTimeKind.Unspecified, ((DateTime)parameter.Value!).Kind);
        Assert.Equal(SqlDbType.SmallDateTime, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void SmallDateTime_accepts_null()
    {
        var parameter = SqlParam.SmallDateTime(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.SmallDateTime, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void DateTime2_uses_default_scale()
    {
        var value = new DateTime(2026, 8, 5, 12, 34, 56, 789, DateTimeKind.Utc);

        var parameter = SqlParam.DateTime2(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(DateTimeKind.Utc, ((DateTime)parameter.Value!).Kind);
        Assert.Equal(SqlDbType.DateTime2, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Equal((byte?)7, parameter.Scale);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void DateTime2_accepts_scale_boundaries(byte scale)
    {
        var parameter = SqlParam.DateTime2(
            new DateTime(2026, 8, 5, 12, 34, 56, 789),
            scale);

        Assert.Equal((byte?)scale, parameter.Scale);
    }

    [Fact]
    public void DateTimeOffset_uses_default_scale()
    {
        var value = new DateTimeOffset(2026, 8, 5, 12, 34, 56, TimeSpan.FromHours(-3));

        var parameter = SqlParam.DateTimeOffset(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(SqlDbType.DateTimeOffset, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Equal((byte?)7, parameter.Scale);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void DateTimeOffset_accepts_scale_boundaries(byte scale)
    {
        var parameter = SqlParam.DateTimeOffset(
            new DateTimeOffset(2026, 8, 5, 12, 34, 56, TimeSpan.FromHours(2)),
            scale);

        Assert.Equal((byte?)scale, parameter.Scale);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8_001)]
    public void VarChar_rejects_invalid_size(int size)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.VarChar("value", size));

        Assert.Equal("size", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8_001)]
    public void Char_rejects_invalid_size(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.Char("value", size));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4_001)]
    public void NVarChar_rejects_invalid_size(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.NVarChar("value", size));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4_001)]
    public void NChar_rejects_invalid_size(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.NChar("value", size));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(byte.MaxValue)]
    public void Time_rejects_invalid_scale(byte scale)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.Time(new TimeOnly(12, 34, 56), scale));

        Assert.Equal("scale", exception.ParamName);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(byte.MaxValue)]
    public void DateTime2_rejects_invalid_scale(byte scale)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.DateTime2(new DateTime(2026, 8, 5), scale));

        Assert.Equal("scale", exception.ParamName);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(byte.MaxValue)]
    public void DateTimeOffset_rejects_invalid_scale(byte scale)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.DateTimeOffset(
                new DateTimeOffset(2026, 8, 5, 12, 34, 56, TimeSpan.Zero),
                scale));

        Assert.Equal("scale", exception.ParamName);
    }

    [Fact]
    public void Fixed_length_factories_accept_sql_server_boundaries()
    {
        var ansi = SqlParam.Char("value", 8_000);
        var unicode = SqlParam.NChar("value", 4_000);

        Assert.Equal(8_000, ansi.Size);
        Assert.Equal(4_000, unicode.Size);
    }

    [Theory]
    [MemberData(nameof(NumericFactories))]
    public void Numeric_factories_create_expected_contract(
        string factoryName,
        SqlDbType expectedSqlDbType)
    {
        ArgumentNullException.ThrowIfNull(factoryName);

        var parameter = CreateNonNullNumericParameter(factoryName);
        object expectedValue = GetExpectedNumericValue(factoryName);

        Assert.NotEmpty(factoryName);
        Assert.Equal(expectedValue, parameter.Value);
        Assert.Equal(expectedSqlDbType, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Theory]
    [MemberData(nameof(NullNumericFactories))]
    public void Numeric_factories_accept_null_values(
        string factoryName,
        SqlDbType expectedSqlDbType)
    {
        ArgumentNullException.ThrowIfNull(factoryName);

        var parameter = CreateNullNumericParameter(factoryName);

        Assert.NotEmpty(factoryName);
        Assert.Null(parameter.Value);
        Assert.Equal(expectedSqlDbType, parameter.SqlDbType);
        Assert.Null(parameter.Size);
    }

    [Fact]
    public void Decimal_creates_expected_contract()
    {
        var parameter = SqlParam.Decimal(123.45M, 18, 2);

        Assert.Equal(123.45M, parameter.Value);
        Assert.Equal(SqlDbType.Decimal, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Equal((byte)18, parameter.Precision);
        Assert.Equal((byte)2, parameter.Scale);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(38, 0)]
    [InlineData(38, 38)]
    public void Decimal_accepts_precision_and_scale_boundaries(
        byte precision,
        byte scale)
    {
        var parameter = SqlParam.Decimal(1M, precision, scale);

        Assert.Equal(precision, parameter.Precision);
        Assert.Equal(scale, parameter.Scale);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(39)]
    public void Decimal_rejects_invalid_precision(byte precision)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.Decimal(1M, precision, 0));

        Assert.Equal("precision", exception.ParamName);
    }

    [Fact]
    public void Decimal_rejects_scale_greater_than_precision()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.Decimal(1M, 18, 19));

        Assert.Equal("scale", exception.ParamName);
    }

    [Fact]
    public void UniqueIdentifier_creates_expected_contract()
    {
        var id = Guid.Parse("f0da086a-cf8d-4682-8a55-e96017890d2b");

        var parameter = SqlParam.UniqueIdentifier(id);

        Assert.Equal(id, parameter.Value);
        Assert.Equal(SqlDbType.UniqueIdentifier, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Precision);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void UniqueIdentifier_accepts_empty_guid()
    {
        var parameter = SqlParam.UniqueIdentifier(Guid.Empty);

        Assert.Equal(Guid.Empty, parameter.Value);
        Assert.Equal(SqlDbType.UniqueIdentifier, parameter.SqlDbType);
    }

    [Fact]
    public void UniqueIdentifier_accepts_null()
    {
        var parameter = SqlParam.UniqueIdentifier(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.UniqueIdentifier, parameter.SqlDbType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8_000)]
    public void Binary_accepts_size_boundaries(int size)
    {
        byte[] value = [0x01, 0x02];

        var parameter = SqlParam.Binary(value, size);

        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.Binary, parameter.SqlDbType);
        Assert.Equal(size, parameter.Size);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8_000)]
    public void VarBinary_accepts_size_boundaries(int size)
    {
        byte[] value = [0x01, 0x02];

        var parameter = SqlParam.VarBinary(value, size);

        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
        Assert.Equal(size, parameter.Size);
    }

    [Fact]
    public void Binary_preserves_empty_array()
    {
        byte[] value = [];

        var parameter = SqlParam.Binary(value, 1);

        Assert.Same(value, parameter.Value);
        Assert.Empty((byte[])parameter.Value!);
        Assert.Equal(SqlDbType.Binary, parameter.SqlDbType);
    }

    [Fact]
    public void VarBinary_preserves_empty_array()
    {
        byte[] value = [];

        var parameter = SqlParam.VarBinary(value, 1);

        Assert.Same(value, parameter.Value);
        Assert.Empty((byte[])parameter.Value!);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
    }

    [Fact]
    public void Binary_accepts_null_array()
    {
        var parameter = SqlParam.Binary(null, 1);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.Binary, parameter.SqlDbType);
        Assert.Equal(1, parameter.Size);
    }

    [Fact]
    public void VarBinary_accepts_null_array()
    {
        var parameter = SqlParam.VarBinary(null, 1);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
        Assert.Equal(1, parameter.Size);
    }

    [Fact]
    public void VarBinaryMax_uses_sql_server_max_size()
    {
        byte[] value = [0x01, 0x02];

        var parameter = SqlParam.VarBinaryMax(value);

        Assert.Same(value, parameter.Value);
        Assert.Equal(SqlDbType.VarBinary, parameter.SqlDbType);
        Assert.Equal(-1, parameter.Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8_001)]
    public void Binary_rejects_invalid_size(int size)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.Binary([0x01], size));

        Assert.Equal("size", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8_001)]
    public void VarBinary_rejects_invalid_size(int size)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SqlParam.VarBinary([0x01], size));

        Assert.Equal("size", exception.ParamName);
    }

    [Fact]
    public void Binary_does_not_validate_value_length_against_size()
    {
        byte[] value = [0x01, 0x02];

        var parameter = SqlParam.Binary(value, 1);

        Assert.Same(value, parameter.Value);
        Assert.Equal(1, parameter.Size);
    }

    [Fact]
    public void SqlParam_public_factories_are_explicit()
    {
        string[] factoryNames = typeof(SqlParam)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(SqlParam))
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "BigInt",
                "Binary",
                "Bit",
                "Char",
                "Date",
                "DateTime",
                "DateTime2",
                "DateTimeOffset",
                "Decimal",
                "Float",
                "Int",
                "Money",
                "NChar",
                "NVarChar",
                "NVarCharMax",
                "Real",
                "SmallDateTime",
                "SmallInt",
                "SmallMoney",
                "TableValued",
                "Time",
                "TinyInt",
                "UniqueIdentifier",
                "VarBinary",
                "VarBinaryMax",
                "VarChar",
                "VarCharMax",
            },
            factoryNames);
    }

    private static TypedSqlParameter CreateNonNullNumericParameter(string factoryName) =>
        factoryName switch
        {
            "Bit" => SqlParam.Bit(true),
            "TinyInt" => SqlParam.TinyInt(byte.MaxValue),
            "SmallInt" => SqlParam.SmallInt(short.MinValue),
            "Int" => SqlParam.Int(int.MinValue),
            "BigInt" => SqlParam.BigInt(long.MinValue),
            "Real" => SqlParam.Real(12.5F),
            "Float" => SqlParam.Float(12.5D),
            "Money" => SqlParam.Money(-12.34M),
            "SmallMoney" => SqlParam.SmallMoney(12.34M),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    public static TypedSqlParameter CreateScalarParameter(string factoryName) =>
        factoryName switch
        {
            "VarChar" => SqlParam.VarChar("value", 20),
            "NVarChar" => SqlParam.NVarChar("value", 20),
            "Char" => SqlParam.Char("A", 1),
            "NChar" => SqlParam.NChar("A", 1),
            "VarCharMax" => SqlParam.VarCharMax("value"),
            "NVarCharMax" => SqlParam.NVarCharMax("value"),
            "Bit" => SqlParam.Bit(true),
            "TinyInt" => SqlParam.TinyInt(1),
            "SmallInt" => SqlParam.SmallInt(1),
            "Int" => SqlParam.Int(1),
            "BigInt" => SqlParam.BigInt(1),
            "Real" => SqlParam.Real(1.5F),
            "Float" => SqlParam.Float(1.5D),
            "Decimal" => SqlParam.Decimal(1.5M, 18, 2),
            "Money" => SqlParam.Money(1.5M),
            "SmallMoney" => SqlParam.SmallMoney(1.5M),
            "UniqueIdentifier" => SqlParam.UniqueIdentifier(Guid.Empty),
            "Binary" => SqlParam.Binary([0x01], 1),
            "VarBinary" => SqlParam.VarBinary([0x01], 1),
            "VarBinaryMax" => SqlParam.VarBinaryMax([0x01]),
            "Date" => SqlParam.Date(new DateOnly(2026, 8, 5)),
            "Time" => SqlParam.Time(new TimeOnly(12, 34, 56)),
            "DateTime" => SqlParam.DateTime(new DateTime(2026, 8, 5)),
            "SmallDateTime" => SqlParam.SmallDateTime(new DateTime(2026, 8, 5)),
            "DateTime2" => SqlParam.DateTime2(new DateTime(2026, 8, 5)),
            "DateTimeOffset" => SqlParam.DateTimeOffset(
                new DateTimeOffset(2026, 8, 5, 12, 34, 56, TimeSpan.Zero)),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private static TypedSqlParameter CreateNullNumericParameter(string factoryName) =>
        factoryName switch
        {
            "Bit" => SqlParam.Bit(null),
            "TinyInt" => SqlParam.TinyInt(null),
            "SmallInt" => SqlParam.SmallInt(null),
            "Int" => SqlParam.Int(null),
            "BigInt" => SqlParam.BigInt(null),
            "Real" => SqlParam.Real(null),
            "Float" => SqlParam.Float(null),
            "Decimal" => SqlParam.Decimal(null, 18, 2),
            "Money" => SqlParam.Money(null),
            "SmallMoney" => SqlParam.SmallMoney(null),
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };

    private static object GetExpectedNumericValue(string factoryName) =>
        factoryName switch
        {
            "Bit" => true,
            "TinyInt" => byte.MaxValue,
            "SmallInt" => short.MinValue,
            "Int" => int.MinValue,
            "BigInt" => long.MinValue,
            "Real" => 12.5F,
            "Float" => 12.5D,
            "Money" => -12.34M,
            "SmallMoney" => 12.34M,
            _ => throw new ArgumentOutOfRangeException(nameof(factoryName)),
        };
}
