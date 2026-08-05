using System.Data;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.Tests;

public sealed class SqlParamTests
{
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

    [Fact]
    public void Date_creates_expected_contract()
    {
        var value = new DateOnly(2026, 8, 5);

        var parameter = SqlParam.Date(value);

        Assert.Equal(value, parameter.Value);
        Assert.Equal(SqlDbType.Date, parameter.SqlDbType);
        Assert.Null(parameter.Size);
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void Date_accepts_null()
    {
        var parameter = SqlParam.Date(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.Date, parameter.SqlDbType);
        Assert.Null(parameter.Size);
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
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void DateTime_accepts_null()
    {
        var parameter = SqlParam.DateTime(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.DateTime, parameter.SqlDbType);
        Assert.Null(parameter.Size);
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
        Assert.Null(parameter.Scale);
    }

    [Fact]
    public void SmallDateTime_accepts_null()
    {
        var parameter = SqlParam.SmallDateTime(null);

        Assert.Null(parameter.Value);
        Assert.Equal(SqlDbType.SmallDateTime, parameter.SqlDbType);
        Assert.Null(parameter.Size);
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
}
