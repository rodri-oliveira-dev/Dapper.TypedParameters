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

. (Join-Path $PSScriptRoot "PackageConsumption.Common.ps1")

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
$packageProfile = Get-PackageProfile -PackageId $PackageId -AssemblyName $AssemblyName
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
        -PackageIdToMap $packageProfile.PackageId `
        -NuGetOrg $NuGetOrgSource

    Write-Host "Package ID: $($packageProfile.PackageId)"
    Write-Host "Package version: $($package.Version)"
    Write-Host "Package file: $($package.Name)"
    Write-Host "Local NuGet source: $resolvedPackageDirectory"
    Write-Host "NuGet.org source: $NuGetOrgSource"
    Write-Host "Isolated NUGET_PACKAGES: $nugetPackages"

    foreach ($targetFramework in @("net8.0", "net10.0")) {
        $consumerDirectory = Join-Path $consumersRoot $targetFramework
        $projectPath = Write-PackageConsumerProject `
            -ProjectDirectory $consumerDirectory `
            -TargetFramework $targetFramework `
            -Profile $packageProfile `
            -Version $package.Version

        Show-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("restore", $projectPath, "--configfile", $nugetConfig, "--packages", $nugetPackages) `
            -WorkingDirectory $consumerDirectory `
            -RetryCount 3

        $lowerPackageId = $packageProfile.PackageId.ToLowerInvariant()
        $lowerVersion = $package.Version.ToLowerInvariant()
        $cachedPackagePath = Join-Path `
            $nugetPackages `
            (Join-Path `
                $lowerPackageId `
                (Join-Path $lowerVersion "$lowerPackageId.$lowerVersion.nupkg"))

        Assert-True (Test-Path -LiteralPath $cachedPackagePath) `
            "Restored package file was not found in isolated cache: $cachedPackagePath"

        $cachedPackageHash = (Get-FileHash -LiteralPath $cachedPackagePath -Algorithm SHA512).Hash
        Assert-True ($cachedPackageHash -eq $packageHash) `
            "Restored package hash does not match the local .nupkg."

        Show-LoggedCommand `
            -FilePath "dotnet" `
            -Arguments @("build", $projectPath, "--configuration", "Release", "--no-restore") `
            -WorkingDirectory $consumerDirectory

        Show-LoggedCommand `
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
