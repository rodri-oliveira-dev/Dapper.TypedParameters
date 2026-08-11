[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PackageVersion,

    [string] $PackageId = "TypedParameters.Dapper.SqlServer",

    [string] $AssemblyName = "Dapper.TypedParameters.SqlServer",

    [string] $NuGetOrgSource = "https://api.nuget.org/v3/index.json",

    [string] $FlatContainerBaseUrl = "https://api.nuget.org/v3-flatcontainer",

    [switch] $KeepArtifacts
)

$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "PackageConsumption.Common.ps1")

function Test-PublicPackageAvailability {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Id,

        [Parameter(Mandatory = $true)]
        [string] $Version,

        [Parameter(Mandatory = $true)]
        [string] $BaseUrl
    )

    $lowerId = $Id.ToLowerInvariant()
    $indexUrl = "$BaseUrl/$lowerId/index.json"

    Write-Host "Checking NuGet.org flat-container: $indexUrl"
    $response = Invoke-RestMethod -Uri $indexUrl
    $versions = @($response.versions)

    Assert-True ($versions -contains $Version) `
        "Package '$Id' version '$Version' was not found at '$indexUrl'."

    return $indexUrl
}

function Write-IsolationFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Workspace,

        [Parameter(Mandatory = $true)]
        [string] $NuGetOrg
    )

    $nugetOrgEscaped = [System.Security.SecurityElement]::Escape($NuGetOrg)
    $nugetConfig = Join-Path $Workspace "NuGet.Config"

    $nugetConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="$nugetOrgEscaped" />
  </packageSources>
</configuration>
"@

    Set-Content -LiteralPath $nugetConfig -Value $nugetConfigContent -Encoding UTF8

    $directoryBuildProps = @"
<Project>
  <PropertyGroup>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
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

    $projectName = "PublicPackageConsumer.$TargetFramework"
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

byte[] payload = new byte[] { 0x01, 0x02 };
var binary = SqlParam.VarBinary(payload, 2);
AssertEqual(SqlDbType.VarBinary, binary.SqlDbType, "varbinary SqlDbType");
AssertEqual((int?)2, binary.Size, "varbinary size");

var date = SqlParam.Date(new DateOnly(2026, 8, 5));
AssertEqual(SqlDbType.Date, date.SqlDbType, "date SqlDbType");

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
    dateTime.AddParameter(command, "Timestamp");

    AssertEqual(8, command.Parameters.Count, "materialized parameter count");
    AssertSqlParameter(command, "Document", SqlDbType.VarChar, expectedSize: 11);
    AssertSqlParameter(command, "Name", SqlDbType.NVarChar, expectedSize: 150);
    AssertSqlParameter(command, "Count", SqlDbType.Int);
    AssertSqlParameter(command, "Amount", SqlDbType.Decimal, expectedPrecision: 18, expectedScale: 2);
    AssertSqlParameter(command, "Id", SqlDbType.UniqueIdentifier);
    AssertSqlParameter(command, "Payload", SqlDbType.VarBinary, expectedSize: 2);
    AssertSqlParameter(command, "DateValue", SqlDbType.Date);
    AssertSqlParameter(command, "Timestamp", SqlDbType.DateTime2, expectedScale: 7);

    AssertEqual(
        new DateTime(2026, 8, 5),
        command.Parameters["DateValue"].Value,
        "DateOnly materialization");
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

function Assert-RestoredFromNuGetOrg {
    param(
        [Parameter(Mandatory = $true)]
        [string] $PackagesDirectory,

        [Parameter(Mandatory = $true)]
        [string] $Id,

        [Parameter(Mandatory = $true)]
        [string] $Version,

        [Parameter(Mandatory = $true)]
        [string] $ExpectedSource
    )

    $lowerId = $Id.ToLowerInvariant()
    $lowerVersion = $Version.ToLowerInvariant()
    $packageDirectory = Join-Path $PackagesDirectory (Join-Path $lowerId $lowerVersion)
    $packageFile = Join-Path $packageDirectory "$lowerId.$lowerVersion.nupkg"
    $metadataFile = Join-Path $packageDirectory ".nupkg.metadata"

    Assert-True (Test-Path -LiteralPath $packageFile) `
        "Restored package file was not found in isolated cache: $packageFile"
    Assert-True (Test-Path -LiteralPath $metadataFile) `
        "NuGet metadata file was not found in isolated cache: $metadataFile"

    $metadata = Get-Content -Raw -LiteralPath $metadataFile | ConvertFrom-Json
    Assert-True ($metadata.source -eq $ExpectedSource) `
        "Restored package source mismatch. Expected '$ExpectedSource', actual '$($metadata.source)'."

    return $packageFile
}

function Assert-AssetsUseExactPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ProjectDirectory,

        [Parameter(Mandatory = $true)]
        [string] $Id,

        [Parameter(Mandatory = $true)]
        [string] $Version
    )

    $assetsPath = Join-Path $ProjectDirectory "obj/project.assets.json"
    Assert-True (Test-Path -LiteralPath $assetsPath) `
        "Restore assets file was not found: $assetsPath"

    $assets = Get-Content -Raw -LiteralPath $assetsPath | ConvertFrom-Json
    $libraryName = "$Id/$Version"
    $library = $assets.libraries.PSObject.Properties |
        Where-Object { $_.Name -eq $libraryName } |
        Select-Object -First 1

    Assert-True ($null -ne $library) `
        "Exact package '$libraryName' was not found in restore assets."
    Assert-True ($library.Value.type -eq "package") `
        "Restore asset '$libraryName' was not restored as a package."
}

$publicIndexUrl = Test-PublicPackageAvailability `
    -Id $PackageId `
    -Version $PackageVersion `
    -BaseUrl $FlatContainerBaseUrl

$artifactsRoot = Join-Path (Get-Location) "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsRoot | Out-Null

$workspace = Join-Path $artifactsRoot ("public-package-consumption-" + [System.Guid]::NewGuid().ToString("N"))
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
        -NuGetOrg $NuGetOrgSource

    Write-Host "Package ID: $PackageId"
    Write-Host "Package version: $PackageVersion"
    Write-Host "NuGet.org source: $NuGetOrgSource"
    Write-Host "Public package index: $publicIndexUrl"
    Write-Host "Isolated workspace: $workspace"
    Write-Host "Isolated NUGET_PACKAGES: $nugetPackages"

    foreach ($targetFramework in @("net8.0", "net10.0")) {
        $consumerDirectory = Join-Path $consumersRoot $targetFramework
        $projectPath = Write-ConsumerProject `
            -ProjectDirectory $consumerDirectory `
            -TargetFramework $targetFramework `
            -Id $PackageId `
            -Version $PackageVersion

        Show-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("restore", $projectPath, "--configfile", $nugetConfig, "--packages", $nugetPackages) `
            -WorkingDirectory $consumerDirectory `
            -RetryCount 3
        Write-Host "$targetFramework restore: passed"

        Assert-AssetsUseExactPackage `
            -ProjectDirectory $consumerDirectory `
            -Id $PackageId `
            -Version $PackageVersion

        $restoredPackage = Assert-RestoredFromNuGetOrg `
            -PackagesDirectory $nugetPackages `
            -Id $PackageId `
            -Version $PackageVersion `
            -ExpectedSource $NuGetOrgSource
        Write-Host "$targetFramework restored package: $restoredPackage"

        Show-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("build", $projectPath, "--configuration", "Release", "--no-restore") `
            -WorkingDirectory $consumerDirectory
        Write-Host "$targetFramework build: passed"

        Show-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("run", "--project", $projectPath, "--configuration", "Release", "--no-build") `
            -WorkingDirectory $consumerDirectory
        Write-Host "$targetFramework execution: passed"
    }
}
finally {
    $env:NUGET_PACKAGES = $previousNuGetPackages

    if (-not $KeepArtifacts) {
        Remove-Item -Recurse -Force -LiteralPath $workspace
    }
    else {
        Write-Host "Kept public package consumption artifacts at: $workspace"
    }
}
