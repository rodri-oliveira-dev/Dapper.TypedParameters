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

$packageProfile = Get-PackageProfile -PackageId $PackageId -AssemblyName $AssemblyName
$publicIndexUrl = Test-PublicPackageAvailability `
    -Id $packageProfile.PackageId `
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

    Write-Host "Package ID: $($packageProfile.PackageId)"
    Write-Host "Package version: $PackageVersion"
    Write-Host "NuGet.org source: $NuGetOrgSource"
    Write-Host "Public package index: $publicIndexUrl"
    Write-Host "Isolated workspace: $workspace"
    Write-Host "Isolated NUGET_PACKAGES: $nugetPackages"

    foreach ($targetFramework in @("net8.0", "net10.0")) {
        $consumerDirectory = Join-Path $consumersRoot $targetFramework
        $projectPath = Write-PackageConsumerProject `
            -ProjectDirectory $consumerDirectory `
            -TargetFramework $targetFramework `
            -Profile $packageProfile `
            -Version $PackageVersion

        Show-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("restore", $projectPath, "--configfile", $nugetConfig, "--packages", $nugetPackages) `
            -WorkingDirectory $consumerDirectory `
            -RetryCount 3
        Write-Host "$targetFramework restore: passed"

        Assert-AssetsUseExactPackage `
            -ProjectDirectory $consumerDirectory `
            -Id $packageProfile.PackageId `
            -Version $PackageVersion

        $restoredPackage = Assert-RestoredFromNuGetOrg `
            -PackagesDirectory $nugetPackages `
            -Id $packageProfile.PackageId `
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
