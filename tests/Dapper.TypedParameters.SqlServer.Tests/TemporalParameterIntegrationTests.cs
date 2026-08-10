using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Dapper.TypedParameters.SqlServer.Tests;

public sealed class TemporalParameterIntegrationTests
{
    private const string ConnectionStringVariable =
        "DAPPER_TYPEDPARAMETERS_SQLSERVER_CONNECTION_STRING";

    [Fact]
    public async Task Temporal_parameters_round_trip_through_dapper()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await AssertNullAsync(connection, SqlParam.Date(null));
        await AssertNullAsync(connection, SqlParam.Time(null, scale: 0));
        await AssertNullAsync(connection, SqlParam.Time(null));
        await AssertNullAsync(connection, SqlParam.DateTime(null));
        await AssertNullAsync(connection, SqlParam.SmallDateTime(null));
        await AssertNullAsync(connection, SqlParam.DateTime2(null, scale: 0));
        await AssertNullAsync(connection, SqlParam.DateTime2(null));
        await AssertNullAsync(connection, SqlParam.DateTimeOffset(null, scale: 0));
        await AssertNullAsync(connection, SqlParam.DateTimeOffset(null));

        var date = new DateOnly(2026, 8, 5);
        var dateResult = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value",
            new { Value = SqlParam.Date(date) });
        Assert.Equal(date.ToDateTime(TimeOnly.MinValue), dateResult);

        var time = new TimeOnly(12, 34, 56).Add(TimeSpan.FromTicks(7_654_321));

        var time0Result = await connection.QuerySingleAsync<TimeSpan>(
            "SELECT @Value",
            new { Value = SqlParam.Time(time, scale: 0) });
        Assert.Equal(RoundTimeSpan(time.ToTimeSpan(), scale: 0), time0Result);

        var time7Result = await connection.QuerySingleAsync<TimeSpan>(
            "SELECT @Value",
            new { Value = SqlParam.Time(time, scale: 7) });
        Assert.Equal(time.ToTimeSpan(), time7Result);

        var dateTime = new DateTime(
            2026,
            8,
            5,
            12,
            34,
            56,
            789,
            DateTimeKind.Unspecified).AddTicks(1_234);
        var dateTimeResult = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value",
            new { Value = SqlParam.DateTime(dateTime) });
        Assert.InRange(
            (dateTimeResult - dateTime).Duration(),
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(4));

        var smallDateTime = new DateTime(2026, 8, 5, 12, 34, 56);
        var smallDateTimeResult = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value",
            new { Value = SqlParam.SmallDateTime(smallDateTime) });
        Assert.Equal(RoundSmallDateTime(smallDateTime), smallDateTimeResult);

        var dateTime2Result0 = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value",
            new { Value = SqlParam.DateTime2(dateTime, scale: 0) });
        Assert.Equal(RoundDateTime(dateTime, scale: 0), dateTime2Result0);

        var dateTime2Result7 = await connection.QuerySingleAsync<DateTime>(
            "SELECT @Value",
            new { Value = SqlParam.DateTime2(dateTime, scale: 7) });
        Assert.Equal(dateTime, dateTime2Result7);

        var positiveOffset = new DateTimeOffset(
            2026,
            8,
            5,
            12,
            34,
            56,
            TimeSpan.FromHours(2)).AddTicks(7_654_321);
        var positiveOffsetResult = await connection.QuerySingleAsync<DateTimeOffset>(
            "SELECT @Value",
            new { Value = SqlParam.DateTimeOffset(positiveOffset, scale: 0) });
        Assert.Equal(RoundDateTimeOffset(positiveOffset, scale: 0), positiveOffsetResult);
        Assert.Equal(positiveOffset.Offset, positiveOffsetResult.Offset);

        var negativeOffset = new DateTimeOffset(
            2026,
            8,
            5,
            12,
            34,
            56,
            TimeSpan.FromHours(-3)).AddTicks(7_654_321);
        var negativeOffsetResult = await connection.QuerySingleAsync<DateTimeOffset>(
            "SELECT @Value",
            new { Value = SqlParam.DateTimeOffset(negativeOffset, scale: 7) });
        Assert.Equal(negativeOffset, negativeOffsetResult);
        Assert.Equal(negativeOffset.Offset, negativeOffsetResult.Offset);
    }

    private static async Task AssertNullAsync(
        SqlConnection connection,
        TypedSqlParameter parameter)
    {
        var isNull = await connection.QuerySingleAsync<bool>(
            "SELECT CONVERT(bit, CASE WHEN @Value IS NULL THEN 1 ELSE 0 END)",
            new { Value = parameter });

        Assert.True(isNull);
    }

    private static TimeSpan RoundTimeSpan(TimeSpan value, int scale) =>
        new(RoundTicks(value.Ticks, scale));

    private static DateTime RoundDateTime(DateTime value, int scale) =>
        new(RoundTicks(value.Ticks, scale), value.Kind);

    private static DateTimeOffset RoundDateTimeOffset(
        DateTimeOffset value,
        int scale) =>
        new(RoundTicks(value.Ticks, scale), value.Offset);

    private static long RoundTicks(long ticks, int scale)
    {
        var factor = (long)Math.Pow(10, 7 - scale);

        return ((ticks + (factor / 2)) / factor) * factor;
    }

    private static DateTime RoundSmallDateTime(DateTime value)
    {
        var truncated = new DateTime(
            value.Year,
            value.Month,
            value.Day,
            value.Hour,
            value.Minute,
            0,
            value.Kind);

        return value.Second >= 30
            ? truncated.AddMinutes(1)
            : truncated;
    }
}
