[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PackageDirectory,

    [string] $PackageId = "TypedParameters.Dapper.SqlServer",

    [string] $AssemblyName = "Dapper.TypedParameters.SqlServer",

    [string] $ExpectedVersion,

    [string] $RepositoryUrl = "https://github.com/rodri-oliveira-dev/Dapper.TypedParameters"
)

$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "PackageConsumption.Common.ps1")

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Expand-Package {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $destination = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Path $destination | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory((Resolve-Path $Path), $destination)

    return $destination
}

function Assert-True {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Assert-EntryExists {
    param(
        [string[]] $Entries,
        [string] $Entry
    )

    Assert-True ($Entries -contains $Entry) "Package entry '$Entry' was not found."
}

function Test-NoForbiddenEntries {
    param(
        [string[]] $Entries,

        [string[]] $ForbiddenAssemblyNames
    )

    $forbidden = @(
        '(^|/)bin/',
        '(^|/)obj/',
        '\.tmp$',
        '\.temp$',
        '\.user$',
        '\.suo$',
        'Dapper\.TypedParameters\.SqlServer\.Tests\.dll$',
        'Dapper\.TypedParameters\.SqlServer\.IntegrationTests\.dll$',
        'Dapper\.TypedParameters\.PostgreSql\.Tests\.dll$',
        'Dapper\.TypedParameters\.PostgreSql\.IntegrationTests\.dll$'
    )

    foreach ($assemblyName in $ForbiddenAssemblyNames) {
        $escapedAssemblyName = [regex]::Escape($assemblyName)
        $forbidden += "(^|/)$escapedAssemblyName\.(dll|xml|pdb)$"
        $forbidden += "(^|/)lib/.+/$escapedAssemblyName\.(dll|xml|pdb)$"
    }

    foreach ($entry in $Entries) {
        foreach ($pattern in $forbidden) {
            Assert-True ($entry -notmatch $pattern) "Forbidden package entry found: $entry"
        }
    }
}

function Test-NoObviousSecrets {
    param(
        [string] $Root,
        [string[]] $Entries
    )

    $secretPattern = '(?i)(password\s*=|pwd\s*=|secret\s*=|api[_-]?key\s*=|token\s*=|BEGIN (RSA |OPENSSH |EC )?PRIVATE KEY)'
    $textExtensions = @('.json', '.xml', '.nuspec', '.txt', '.md', '.props', '.targets', '.ps1', '.yml', '.yaml')

    foreach ($entry in $Entries) {
        $extension = [System.IO.Path]::GetExtension($entry)
        if ($textExtensions -notcontains $extension) {
            continue
        }

        $path = Join-Path $Root $entry
        $content = Get-Content -Raw -LiteralPath $path
        Assert-True ($content -notmatch $secretPattern) "Potential secret pattern found in package entry: $entry"
    }
}

function Test-SourceLinkMetadata {
    param(
        [string] $Root,
        [string[]] $Entries,
        [string] $ExpectedRepositoryUrl
    )

    foreach ($tfm in @('net8.0', 'net10.0')) {
        $pdbEntry = "lib/$tfm/$AssemblyName.pdb"
        Assert-EntryExists $Entries $pdbEntry

        $pdbPath = Join-Path $Root $pdbEntry
        $bytes = [System.IO.File]::ReadAllBytes($pdbPath)
        $text = [System.Text.Encoding]::UTF8.GetString($bytes)

        Assert-True (
            $text.Contains($ExpectedRepositoryUrl) -or
            $text.Contains("raw.githubusercontent.com/rodri-oliveira-dev/Dapper.TypedParameters")
        ) "SourceLink metadata was not found in $pdbEntry."
    }
}

function Get-ExactArtifact {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Directory,

        [Parameter(Mandatory = $true)]
        [string] $Id,

        [Parameter(Mandatory = $true)]
        [string] $Extension
    )

    $escapedPackageId = [regex]::Escape($Id)
    $escapedExtension = [regex]::Escape($Extension)
    $artifactPattern = "^$escapedPackageId\.(?<Version>.+)$escapedExtension$"
    $artifacts = @(Get-ChildItem -LiteralPath $Directory -File -Filter "*$Extension" |
        Where-Object { $_.Name -match $artifactPattern })

    if ($artifacts.Count -ne 1) {
        $found = if ($artifacts.Count -eq 0) {
            "<none>"
        }
        else {
            $artifacts.Name -join ", "
        }

        throw "Expected exactly one $Id $Extension in '$Directory', found $($artifacts.Count): $found"
    }

    $version = [regex]::Match($artifacts[0].Name, $artifactPattern).Groups["Version"].Value

    return [pscustomobject]@{
        Path = $artifacts[0].FullName
        Name = $artifacts[0].Name
        Version = $version
    }
}

$packageProfile = Get-PackageProfile -PackageId $PackageId -AssemblyName $AssemblyName
$packagePath = Get-ExactArtifact -Directory $PackageDirectory -Id $PackageId -Extension ".nupkg"
$symbolPath = Get-ExactArtifact -Directory $PackageDirectory -Id $PackageId -Extension ".snupkg"

Assert-True ($packagePath.Version -eq $symbolPath.Version) `
    "Package and symbol package versions differ. Package='$($packagePath.Version)', symbols='$($symbolPath.Version)'."

if (-not [string]::IsNullOrWhiteSpace($ExpectedVersion)) {
    Assert-True ($packagePath.Version -eq $ExpectedVersion) `
        "Unexpected package version '$($packagePath.Version)'. Expected '$ExpectedVersion'."
}

$packageRoot = Expand-Package $packagePath.Path
$symbolRoot = Expand-Package $symbolPath.Path

try {
    $entries = Get-ChildItem -Recurse -File -LiteralPath $packageRoot |
        ForEach-Object { $_.FullName.Substring($packageRoot.Length + 1).Replace('\', '/') }
    $symbolEntries = Get-ChildItem -Recurse -File -LiteralPath $symbolRoot |
        ForEach-Object { $_.FullName.Substring($symbolRoot.Length + 1).Replace('\', '/') }

    foreach ($tfm in @('net8.0', 'net10.0')) {
        Assert-EntryExists $entries "lib/$tfm/$AssemblyName.dll"
        Assert-EntryExists $entries "lib/$tfm/$AssemblyName.xml"
        Assert-EntryExists $symbolEntries "lib/$tfm/$AssemblyName.pdb"
    }

    Assert-EntryExists $entries "README.md"
    Assert-EntryExists $entries "nuget-icon.png"
    Test-NoForbiddenEntries $entries $packageProfile.ForbiddenAssemblyNames
    Test-NoForbiddenEntries $symbolEntries $packageProfile.ForbiddenAssemblyNames
    Test-NoObviousSecrets $packageRoot $entries
    Test-NoObviousSecrets $symbolRoot $symbolEntries

    $nuspecEntry = $entries | Where-Object { $_ -like '*.nuspec' } | Select-Object -First 1
    Assert-True (-not [string]::IsNullOrWhiteSpace($nuspecEntry)) "No .nuspec entry was found."

    [xml] $nuspec = Get-Content -Raw -LiteralPath (Join-Path $packageRoot $nuspecEntry)
    $metadata = $nuspec.package.metadata

    Assert-True ($metadata.id -eq $PackageId) "Unexpected package id '$($metadata.id)'."
    Assert-True ($metadata.license.type -eq 'expression') "License expression metadata was not found."
    Assert-True ($metadata.license.'#text' -eq 'MIT') "MIT license expression was not found."
    Assert-True ($metadata.readme -eq 'README.md') "README metadata was not found."
    Assert-True ($metadata.icon -eq 'nuget-icon.png') "Package icon metadata was not found."
    Assert-True ($metadata.repository.url -eq $RepositoryUrl) "Repository URL metadata was not found."

    $readmeContent = Get-Content -Raw -LiteralPath (Join-Path $packageRoot "README.md")
    Assert-True ($readmeContent.Contains($packageProfile.ExpectedReadmeHeading)) `
        "Package README does not contain expected heading '$($packageProfile.ExpectedReadmeHeading)'."
    Assert-True (-not $readmeContent.Contains($packageProfile.ForbiddenReadmeHeading)) `
        "Package README contains provider heading '$($packageProfile.ForbiddenReadmeHeading)'."

    $dependencyIds = @($metadata.dependencies.group.dependency | ForEach-Object { $_.id })
    foreach ($dependencyId in $packageProfile.ExpectedDependencies) {
        Assert-True ($dependencyIds -contains $dependencyId) `
            "$dependencyId dependency metadata was not found."
    }

    foreach ($dependencyId in $packageProfile.ForbiddenDependencies) {
        Assert-True ($dependencyIds -notcontains $dependencyId) `
            "Forbidden dependency metadata was found: $dependencyId."
    }

    Test-SourceLinkMetadata $symbolRoot $symbolEntries $RepositoryUrl

    [pscustomobject]@{
        Package = $packagePath.Name
        SymbolPackage = $symbolPath.Name
        PackageEntries = $entries.Count
        SymbolEntries = $symbolEntries.Count
        Frameworks = 'net8.0, net10.0'
        Dependencies = ($dependencyIds -join ', ')
    } | Format-List
}
finally {
    Remove-Item -Recurse -Force -LiteralPath $packageRoot
    Remove-Item -Recurse -Force -LiteralPath $symbolRoot
}
