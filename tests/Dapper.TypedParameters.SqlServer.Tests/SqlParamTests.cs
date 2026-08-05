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

    [Fact]
    public void Fixed_length_factories_accept_sql_server_boundaries()
    {
        var ansi = SqlParam.Char("value", 8_000);
        var unicode = SqlParam.NChar("value", 4_000);

        Assert.Equal(8_000, ansi.Size);
        Assert.Equal(4_000, unicode.Size);
    }
}
