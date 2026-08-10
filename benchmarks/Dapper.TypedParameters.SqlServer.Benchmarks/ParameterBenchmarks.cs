using System.Data;
using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Dapper.TypedParameters.SqlServer.Benchmarks;

[MemoryDiagnoser]
public class ParameterBenchmarks
{
    private readonly byte[] bytes = new byte[32];
    private readonly DataTable tvp = CreateSmallTvp();

    [Benchmark]
    public TypedSqlParameter CreateStringParameter() =>
        SqlParam.VarChar("document-001", 32);

    [Benchmark]
    public TypedSqlParameter CreateDecimalParameter() =>
        SqlParam.Decimal(1234.56m, precision: 18, scale: 2);

    [Benchmark]
    public TypedSqlParameter CreateBinaryParameter() =>
        SqlParam.VarBinary(bytes, bytes.Length);

    [Benchmark]
    public TableValuedSqlParameter CreateSmallTableValuedParameter() =>
        SqlParam.TableValued("dbo.BenchmarkRows", tvp);

    [Benchmark]
    public int MaterializeStringParameter() =>
        Materialize(SqlParam.VarChar("document-001", 32), "Document").Size;

    [Benchmark]
    public byte MaterializeDecimalParameter() =>
        Materialize(SqlParam.Decimal(1234.56m, precision: 18, scale: 2), "Amount").Precision;

    [Benchmark]
    public int MaterializeBinaryParameter() =>
        Materialize(SqlParam.VarBinary(bytes, bytes.Length), "Payload").Size;

    [Benchmark]
    public string MaterializeSmallTableValuedParameter() =>
        Materialize(SqlParam.TableValued("dbo.BenchmarkRows", tvp), "Rows").TypeName;

    private static SqlParameter Materialize(
        SqlMapper.ICustomQueryParameter parameter,
        string name)
    {
        using var command = new SqlCommand();
        parameter.AddParameter(command, name);

        return command.Parameters[name];
    }

    private static DataTable CreateSmallTvp()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Ada");
        table.Rows.Add(2, "Grace");

        return table;
    }
}
