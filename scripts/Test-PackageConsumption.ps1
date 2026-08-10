[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PackageDirectory,

    [string] $PackageId = "TypedParameters.Dapper.SqlServer",

    [string] $AssemblyName = "Dapper.TypedParameters.SqlServer",

    [string] $NuGetOrgSource = "https://api.nuget.org/v3/index.json",

    [switch] $KeepArtifacts
)

$ErrorActionPreference = "Stop"

function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-LoggedCommand {
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

function Get-ExactPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Directory,

        [Parameter(Mandatory = $true)]
        [string] $Id
    )

    $escapedPackageId = [regex]::Escape($Id)
    $packagePattern = "^$escapedPackageId\.(?<Version>.+)\.nupkg$"
    $packages = @(Get-ChildItem -LiteralPath $Directory -File -Filter "*.nupkg" |
        Where-Object { $_.Name -match $packagePattern })

    if ($packages.Count -ne 1) {
        $found = if ($packages.Count -eq 0) {
            "<none>"
        }
        else {
            $packages.Name -join ", "
        }

        throw "Expected exactly one $Id .nupkg in '$Directory', found $($packages.Count): $found"
    }

    $version = [regex]::Match($packages[0].Name, $packagePattern).Groups["Version"].Value

    return [pscustomobject]@{
        Path = $packages[0].FullName
        Name = $packages[0].Name
        Version = $version
    }
}

function Write-ConsumerProject {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ProjectDirectory,

        [Parameter(Mandatory = $true)]
        [string] $TargetFramework,

        [Parameter(Mandatory = $true)]
        [string] $Id,

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
    <PackageReference Include="$Id" Version="$Version" />
  </ItemGroup>
</Project>
"@

    Set-Content -LiteralPath (Join-Path $ProjectDirectory "$projectName.csproj") -Value $projectContent -Encoding UTF8

    $programContent = @"
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

var amount = SqlParam.Decimal(123.45M, 18, 2);
AssertEqual(SqlDbType.Decimal, amount.SqlDbType, "decimal SqlDbType");
AssertEqual((byte?)18, amount.Precision, "decimal precision");
AssertEqual((byte?)2, amount.Scale, "decimal scale");

var count = SqlParam.Int(42);
AssertEqual(SqlDbType.Int, count.SqlDbType, "int SqlDbType");

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
    amount.AddParameter(command, "Amount");
    binary.AddParameter(command, "Payload");
    date.AddParameter(command, "DateValue");
    time.AddParameter(command, "TimeValue");

    AssertEqual(5, command.Parameters.Count, "materialized parameter count");
    AssertSqlParameter(command, "Document", SqlDbType.VarChar, 11);
    AssertSqlParameter(command, "Amount", SqlDbType.Decimal, expectedPrecision: 18, expectedScale: 2);
    AssertSqlParameter(command, "Payload", SqlDbType.VarBinary, 2);
    AssertSqlParameter(command, "DateValue", SqlDbType.Date);
    AssertSqlParameter(command, "TimeValue", SqlDbType.Time, expectedScale: 3);

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

Console.WriteLine("$TargetFramework consumer: passed");

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

static void AssertSame(object expected, object actual, string name)
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

    Set-Content -LiteralPath (Join-Path $ProjectDirectory "Program.cs") -Value $programContent -Encoding UTF8

    return Join-Path $ProjectDirectory "$projectName.csproj"
}

function Write-IsolationFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Workspace,

        [Parameter(Mandatory = $true)]
        [string] $LocalSource,

        [Parameter(Mandatory = $true)]
        [string] $PackageIdToMap,

        [Parameter(Mandatory = $true)]
        [string] $NuGetOrg
    )

    $localSourceEscaped = [System.Security.SecurityElement]::Escape($LocalSource)
    $packageIdEscaped = [System.Security.SecurityElement]::Escape($PackageIdToMap)
    $nugetOrgEscaped = [System.Security.SecurityElement]::Escape($NuGetOrg)
    $nugetConfig = Join-Path $Workspace "NuGet.Config"

    $nugetConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-package" value="$localSourceEscaped" />
    <add key="nuget.org" value="$nugetOrgEscaped" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local-package">
      <package pattern="$packageIdEscaped" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@

    Set-Content -LiteralPath $nugetConfig -Value $nugetConfigContent -Encoding UTF8

    $directoryBuildProps = @"
<Project>
  <PropertyGroup>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"@

    Set-Content -LiteralPath (Join-Path $Workspace "Directory.Build.props") -Value $directoryBuildProps -Encoding UTF8

    $directoryPackagesProps = @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
"@

    Set-Content -LiteralPath (Join-Path $Workspace "Directory.Packages.props") -Value $directoryPackagesProps -Encoding UTF8

    return $nugetConfig
}

$resolvedPackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path
$package = Get-ExactPackage -Directory $resolvedPackageDirectory -Id $PackageId
$packageHash = (Get-FileHash -LiteralPath $package.Path -Algorithm SHA512).Hash

$artifactsRoot = Join-Path (Get-Location) "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsRoot | Out-Null

$workspace = Join-Path $artifactsRoot ("package-consumption-" + [System.Guid]::NewGuid().ToString("N"))
$nugetPackages = Join-Path $workspace ".nuget-packages"
$consumersRoot = Join-Path $workspace "consumers"

New-Item -ItemType Directory -Force -Path $workspace | Out-Null
New-Item -ItemType Directory -Force -Path $nugetPackages | Out-Null
New-Item -ItemType Directory -Force -Path $consumersRoot | Out-Null

$previousNuGetPackages = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = $nugetPackages

try {
    $nugetConfig = Write-IsolationFiles `
        -Workspace $workspace `
        -LocalSource $resolvedPackageDirectory `
        -PackageIdToMap $PackageId `
        -NuGetOrg $NuGetOrgSource

    Write-Host "Package ID: $PackageId"
    Write-Host "Package version: $($package.Version)"
    Write-Host "Package file: $($package.Name)"
    Write-Host "Local NuGet source: $resolvedPackageDirectory"
    Write-Host "NuGet.org source: $NuGetOrgSource"
    Write-Host "Isolated NUGET_PACKAGES: $nugetPackages"

    foreach ($targetFramework in @("net8.0", "net10.0")) {
        $consumerDirectory = Join-Path $consumersRoot $targetFramework
        $projectPath = Write-ConsumerProject `
            -ProjectDirectory $consumerDirectory `
            -TargetFramework $targetFramework `
            -Id $PackageId `
            -Version $package.Version

        Invoke-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("restore", $projectPath, "--configfile", $nugetConfig, "--packages", $nugetPackages) `
            -WorkingDirectory $consumerDirectory `
            -RetryCount 3

        $cachedPackagePath = Join-Path `
            $nugetPackages `
            (Join-Path `
                $PackageId.ToLowerInvariant() `
                (Join-Path $package.Version.ToLowerInvariant() "$($PackageId.ToLowerInvariant()).$($package.Version.ToLowerInvariant()).nupkg"))

        Assert-True (Test-Path -LiteralPath $cachedPackagePath) `
            "Restored package file was not found in isolated cache: $cachedPackagePath"

        $cachedPackageHash = (Get-FileHash -LiteralPath $cachedPackagePath -Algorithm SHA512).Hash
        Assert-True ($cachedPackageHash -eq $packageHash) `
            "Restored package hash does not match the local .nupkg."

        Invoke-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("build", $projectPath, "--configuration", "Release", "--no-restore") `
            -WorkingDirectory $consumerDirectory

        Invoke-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("run", "--project", $projectPath, "--configuration", "Release", "--no-build") `
            -WorkingDirectory $consumerDirectory
    }
}
finally {
    $env:NUGET_PACKAGES = $previousNuGetPackages

    if (-not $KeepArtifacts) {
        Remove-Item -Recurse -Force -LiteralPath $workspace
    }
    else {
        Write-Host "Kept package consumption artifacts at: $workspace"
    }
}
