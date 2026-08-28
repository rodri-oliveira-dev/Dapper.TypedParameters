function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Show-LoggedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FilePath,

        [Parameter(Mandatory = $true)]
        [string[]] $Arguments,

        [Parameter(Mandatory = $true)]
        [string] $WorkingDirectory,

        [int] $RetryCount = 1
    )

    for ($attempt = 1; $attempt -le $RetryCount; $attempt++) {
        Write-Host "> $FilePath $($Arguments -join ' ')"
        Push-Location -LiteralPath $WorkingDirectory
        try {
            & $FilePath @Arguments
        }
        finally {
            Pop-Location
        }

        if ($LASTEXITCODE -eq 0) {
            return
        }

        if ($attempt -lt $RetryCount) {
            Write-Warning "Command failed with exit code $LASTEXITCODE. Retrying ($($attempt + 1)/$RetryCount)..."
            Start-Sleep -Seconds (2 * $attempt)
        }
    }

    throw "Command failed with exit code $LASTEXITCODE."
}

function Get-PackageProfile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $PackageId,

        [string] $AssemblyName
    )

    $profiles = @{
        "TypedParameters.Dapper.SqlServer" = [pscustomobject]@{
            PackageId = "TypedParameters.Dapper.SqlServer"
            AssemblyName = "Dapper.TypedParameters.SqlServer"
            ExpectedReadmeHeading = "# TypedParameters.Dapper.SqlServer"
            ForbiddenReadmeHeading = "# TypedParameters.Dapper.PostgreSql"
            ExpectedDependencies = @("Dapper", "Microsoft.Data.SqlClient")
            ForbiddenDependencies = @("Npgsql", "TypedParameters.Dapper.PostgreSql")
            ForbiddenAssemblyNames = @("Dapper.TypedParameters.PostgreSql")
        }
        "TypedParameters.Dapper.PostgreSql" = [pscustomobject]@{
            PackageId = "TypedParameters.Dapper.PostgreSql"
            AssemblyName = "Dapper.TypedParameters.PostgreSql"
            ExpectedReadmeHeading = "# TypedParameters.Dapper.PostgreSql"
            ForbiddenReadmeHeading = "# TypedParameters.Dapper.SqlServer"
            ExpectedDependencies = @("Dapper", "Npgsql")
            ForbiddenDependencies = @("Microsoft.Data.SqlClient", "TypedParameters.Dapper.SqlServer")
            ForbiddenAssemblyNames = @("Dapper.TypedParameters.SqlServer")
        }
    }

    Assert-True ($profiles.ContainsKey($PackageId)) `
        "Unsupported package id '$PackageId'. Expected one of: $($profiles.Keys -join ', ')."

    $profile = $profiles[$PackageId]
    if (-not [string]::IsNullOrWhiteSpace($AssemblyName)) {
        Assert-True ($AssemblyName -eq $profile.AssemblyName) `
            "AssemblyName '$AssemblyName' does not match package '$PackageId'. Expected '$($profile.AssemblyName)'."
    }

    return $profile
}

function New-ConsumerProgramContent {
    param(
        [Parameter(Mandatory = $true)]
        [object] $Profile,

        [Parameter(Mandatory = $true)]
        [string] $TargetFramework
    )

    if ($Profile.PackageId -eq "TypedParameters.Dapper.SqlServer") {
        return @"
using System.Data;
using Dapper;
using Dapper.TypedParameters.SqlServer;
using Microsoft.Data.SqlClient;

var publicType = Type.GetType(
    "Dapper.TypedParameters.SqlServer.SqlParam, Dapper.TypedParameters.SqlServer",
    throwOnError: true);
AssertTrue(publicType is not null, "SqlParam public type was not found.");

AssertAssignable<SqlMapper.ICustomQueryParameter>(SqlParam.VarChar("12345678901", 11));

var document = SqlParam.VarChar("12345678901", 11);
AssertEqual("12345678901", document.Value, "varchar value");
AssertEqual(SqlDbType.VarChar, document.SqlDbType, "varchar SqlDbType");
AssertEqual((int?)11, document.Size, "varchar size");

var name = SqlParam.NVarChar("Rodrigo", 150);
AssertEqual("Rodrigo", name.Value, "nvarchar value");
AssertEqual(SqlDbType.NVarChar, name.SqlDbType, "nvarchar SqlDbType");
AssertEqual((int?)150, name.Size, "nvarchar size");

var count = SqlParam.Int(42);
AssertEqual(SqlDbType.Int, count.SqlDbType, "int SqlDbType");
AssertEqual(42, count.Value, "int value");

var amount = SqlParam.Decimal(123.45M, 18, 2);
AssertEqual(SqlDbType.Decimal, amount.SqlDbType, "decimal SqlDbType");
AssertEqual((byte?)18, amount.Precision, "decimal precision");
AssertEqual((byte?)2, amount.Scale, "decimal scale");

var id = Guid.Parse("7cdb49ea-c947-4fe1-861b-ddd941a02422");
var uniqueIdentifier = SqlParam.UniqueIdentifier(id);
AssertEqual(SqlDbType.UniqueIdentifier, uniqueIdentifier.SqlDbType, "uniqueidentifier SqlDbType");
AssertEqual(id, uniqueIdentifier.Value, "uniqueidentifier value");

byte[] payload = [0x01, 0x02];
var binary = SqlParam.VarBinary(payload, 2);
AssertEqual(SqlDbType.VarBinary, binary.SqlDbType, "varbinary SqlDbType");
AssertEqual((int?)2, binary.Size, "varbinary size");

var date = SqlParam.Date(new DateOnly(2026, 8, 5));
AssertEqual(SqlDbType.Date, date.SqlDbType, "date SqlDbType");

var time = SqlParam.Time(new TimeOnly(12, 34, 56), scale: 3);
AssertEqual(SqlDbType.Time, time.SqlDbType, "time SqlDbType");
AssertEqual((byte?)3, time.Scale, "time scale");

var dateTime = SqlParam.DateTime2(
    new DateTime(2026, 8, 5, 12, 34, 56, DateTimeKind.Utc),
    scale: 7);
AssertEqual(SqlDbType.DateTime2, dateTime.SqlDbType, "datetime2 SqlDbType");
AssertEqual((byte?)7, dateTime.Scale, "datetime2 scale");

using (var command = new SqlCommand())
{
    document.AddParameter(command, "Document");
    name.AddParameter(command, "Name");
    count.AddParameter(command, "Count");
    amount.AddParameter(command, "Amount");
    uniqueIdentifier.AddParameter(command, "Id");
    binary.AddParameter(command, "Payload");
    date.AddParameter(command, "DateValue");
    time.AddParameter(command, "TimeValue");
    dateTime.AddParameter(command, "Timestamp");

    AssertEqual(9, command.Parameters.Count, "materialized parameter count");
    AssertSqlParameter(command, "Document", SqlDbType.VarChar, expectedSize: 11);
    AssertSqlParameter(command, "Name", SqlDbType.NVarChar, expectedSize: 150);
    AssertSqlParameter(command, "Count", SqlDbType.Int);
    AssertSqlParameter(command, "Amount", SqlDbType.Decimal, expectedPrecision: 18, expectedScale: 2);
    AssertSqlParameter(command, "Id", SqlDbType.UniqueIdentifier);
    AssertSqlParameter(command, "Payload", SqlDbType.VarBinary, expectedSize: 2);
    AssertSqlParameter(command, "DateValue", SqlDbType.Date);
    AssertSqlParameter(command, "TimeValue", SqlDbType.Time, expectedScale: 3);
    AssertSqlParameter(command, "Timestamp", SqlDbType.DateTime2, expectedScale: 7);

    AssertEqual(
        new DateTime(2026, 8, 5),
        command.Parameters["DateValue"].Value,
        "DateOnly materialization");
    AssertEqual(
        new TimeOnly(12, 34, 56).ToTimeSpan(),
        command.Parameters["TimeValue"].Value,
        "TimeOnly materialization");
}

using (var outputCommand = new SqlCommand())
{
    var output = SqlParam.Int(null).AsOutput();
    output.AddParameter(outputCommand, "Total");
    AssertEqual(ParameterDirection.Output, outputCommand.Parameters["Total"].Direction, "output direction");
    outputCommand.Parameters["Total"].Value = 42;
    AssertEqual(42, output.OutputValue, "output value");
    AssertEqual(42, output.GetValue<int>(), "typed output value");
}

using (var inputOutputCommand = new SqlCommand())
{
    var inputOutput = SqlParam.NVarChar("initial", 20).AsInputOutput();
    inputOutput.AddParameter(inputOutputCommand, "Name");
    AssertEqual(ParameterDirection.InputOutput, inputOutputCommand.Parameters["Name"].Direction, "input/output direction");
    AssertEqual("initial", inputOutputCommand.Parameters["Name"].Value, "input/output initial value");
}

using (var table = new DataTable())
using (var tvpCommand = new SqlCommand())
{
    table.Columns.Add("Id", typeof(int));
    table.Columns.Add("Name", typeof(string));
    table.Rows.Add(1, "First");

    var tvp = SqlParam.TableValued("dbo.ItemList", table);
    AssertAssignable<SqlMapper.ICustomQueryParameter>(tvp);
    AssertEqual(SqlDbType.Structured, tvp.SqlDbType, "TVP SqlDbType");
    AssertEqual("dbo.ItemList", tvp.TypeName, "TVP type name");

    tvp.AddParameter(tvpCommand, "Items");

    var sqlParameter = tvpCommand.Parameters["Items"];
    AssertEqual(SqlDbType.Structured, sqlParameter.SqlDbType, "materialized TVP SqlDbType");
    AssertEqual("dbo.ItemList", sqlParameter.TypeName, "materialized TVP type name");
    AssertSame(table, sqlParameter.Value, "materialized TVP value");
}

Console.WriteLine("$TargetFramework SQL Server consumer: passed");

static void AssertSqlParameter(
    SqlCommand command,
    string name,
    SqlDbType expectedType,
    int? expectedSize = null,
    byte? expectedPrecision = null,
    byte? expectedScale = null)
{
    var parameter = command.Parameters[name];
    AssertEqual(expectedType, parameter.SqlDbType, name + " SqlDbType");

    if (expectedSize.HasValue)
    {
        AssertEqual(expectedSize.Value, parameter.Size, name + " size");
    }

    if (expectedPrecision.HasValue)
    {
        AssertEqual(expectedPrecision.Value, parameter.Precision, name + " precision");
    }

    if (expectedScale.HasValue)
    {
        AssertEqual(expectedScale.Value, parameter.Scale, name + " scale");
    }
}

static void AssertAssignable<T>(object value)
{
    if (value is not T)
    {
        throw new InvalidOperationException(
            "Expected value to be assignable to " + typeof(T).FullName +
            ", actual type was " + value.GetType().FullName + ".");
    }
}

static void AssertEqual<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException(
            name + " mismatch. Expected '" + expected + "', actual '" + actual + "'.");
    }
}

static void AssertSame(object expected, object? actual, string name)
{
    if (!ReferenceEquals(expected, actual))
    {
        throw new InvalidOperationException(name + " did not preserve object identity.");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
"@
    }

    return @"
using Dapper;
using Dapper.TypedParameters.PostgreSql;
using Npgsql;
using NpgsqlTypes;

var publicType = Type.GetType(
    "Dapper.TypedParameters.PostgreSql.PostgresParam, Dapper.TypedParameters.PostgreSql",
    throwOnError: true);
AssertTrue(publicType is not null, "PostgresParam public type was not found.");

AssertAssignable<SqlMapper.ICustomQueryParameter>(PostgresParam.VarChar("customer-001"));

var document = PostgresParam.VarChar("customer-001");
AssertEqual("customer-001", document.Value, "varchar value");
AssertEqual(NpgsqlDbType.Varchar, document.NpgsqlDbType, "varchar NpgsqlDbType");

var payload = PostgresParam.Jsonb("{\"active\":true}");
AssertEqual(NpgsqlDbType.Jsonb, payload.NpgsqlDbType, "jsonb NpgsqlDbType");

var amount = PostgresParam.Numeric(123.45M);
AssertEqual(NpgsqlDbType.Numeric, amount.NpgsqlDbType, "numeric NpgsqlDbType");

var id = Guid.Parse("7cdb49ea-c947-4fe1-861b-ddd941a02422");
var uuid = PostgresParam.Uuid(id);
AssertEqual(NpgsqlDbType.Uuid, uuid.NpgsqlDbType, "uuid NpgsqlDbType");
AssertEqual(id, uuid.Value, "uuid value");

byte[] binaryValue = [0x01, 0x02];
var binary = PostgresParam.Bytea(binaryValue);
AssertEqual(NpgsqlDbType.Bytea, binary.NpgsqlDbType, "bytea NpgsqlDbType");

var date = PostgresParam.Date(new DateOnly(2026, 8, 5));
AssertEqual(NpgsqlDbType.Date, date.NpgsqlDbType, "date NpgsqlDbType");

var timestamp = PostgresParam.Timestamp(new DateTime(2026, 8, 5, 12, 34, 56, DateTimeKind.Unspecified));
AssertEqual(NpgsqlDbType.Timestamp, timestamp.NpgsqlDbType, "timestamp NpgsqlDbType");

var timestampTz = PostgresParam.TimestampTz(new DateTime(2026, 8, 5, 12, 34, 56, DateTimeKind.Utc));
AssertEqual(NpgsqlDbType.TimestampTz, timestampTz.NpgsqlDbType, "timestamptz NpgsqlDbType");

var ids = PostgresParam.Array(new List<int> { 1, 2 }, NpgsqlDbType.Integer);
AssertEqual(NpgsqlDbType.Array | NpgsqlDbType.Integer, ids.NpgsqlDbType, "array NpgsqlDbType");

using (var command = new NpgsqlCommand())
{
    document.AddParameter(command, "Document");
    payload.AddParameter(command, "Payload");
    amount.AddParameter(command, "Amount");
    uuid.AddParameter(command, "Id");
    binary.AddParameter(command, "BinaryValue");
    date.AddParameter(command, "DateValue");
    timestamp.AddParameter(command, "TimestampValue");
    timestampTz.AddParameter(command, "TimestampTzValue");
    ids.AddParameter(command, "Ids");
    PostgresParam.Text(null).AddParameter(command, "MissingText");

    AssertEqual(10, command.Parameters.Count, "materialized parameter count");
    AssertPostgresParameter(command, "Document", NpgsqlDbType.Varchar);
    AssertPostgresParameter(command, "Payload", NpgsqlDbType.Jsonb);
    AssertPostgresParameter(command, "Amount", NpgsqlDbType.Numeric);
    AssertPostgresParameter(command, "Id", NpgsqlDbType.Uuid);
    AssertPostgresParameter(command, "BinaryValue", NpgsqlDbType.Bytea);
    AssertPostgresParameter(command, "DateValue", NpgsqlDbType.Date);
    AssertPostgresParameter(command, "TimestampValue", NpgsqlDbType.Timestamp);
    AssertPostgresParameter(command, "TimestampTzValue", NpgsqlDbType.TimestampTz);
    AssertPostgresParameter(command, "Ids", NpgsqlDbType.Array | NpgsqlDbType.Integer);
    AssertPostgresParameter(command, "MissingText", NpgsqlDbType.Text);
    AssertSame(DBNull.Value, command.Parameters["MissingText"].Value, "null materialization");
}

Console.WriteLine("$TargetFramework PostgreSQL consumer: passed");

static void AssertPostgresParameter(
    NpgsqlCommand command,
    string name,
    NpgsqlDbType expectedType)
{
    var parameter = command.Parameters[name];
    AssertEqual(expectedType, parameter.NpgsqlDbType, name + " NpgsqlDbType");
}

static void AssertAssignable<T>(object value)
{
    if (value is not T)
    {
        throw new InvalidOperationException(
            "Expected value to be assignable to " + typeof(T).FullName +
            ", actual type was " + value.GetType().FullName + ".");
    }
}

static void AssertEqual<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException(
            name + " mismatch. Expected '" + expected + "', actual '" + actual + "'.");
    }
}

static void AssertSame(object expected, object? actual, string name)
{
    if (!ReferenceEquals(expected, actual))
    {
        throw new InvalidOperationException(name + " did not preserve object identity.");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
"@
}

function Write-PackageConsumerProject {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ProjectDirectory,

        [Parameter(Mandatory = $true)]
        [string] $TargetFramework,

        [Parameter(Mandatory = $true)]
        [object] $Profile,

        [Parameter(Mandatory = $true)]
        [string] $Version
    )

    New-Item -ItemType Directory -Force -Path $ProjectDirectory | Out-Null

    $projectName = "PackageConsumer.$TargetFramework"
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>$TargetFramework</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="$($Profile.PackageId)" Version="$Version" />
  </ItemGroup>
</Project>
"@

    Set-Content -LiteralPath (Join-Path $ProjectDirectory "$projectName.csproj") -Value $projectContent -Encoding UTF8

    $programContent = New-ConsumerProgramContent -Profile $Profile -TargetFramework $TargetFramework
    Set-Content -LiteralPath (Join-Path $ProjectDirectory "Program.cs") -Value $programContent -Encoding UTF8

    return Join-Path $ProjectDirectory "$projectName.csproj"
}
