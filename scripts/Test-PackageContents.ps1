[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PackageDirectory,

    [string] $PackageId = "TypedParameters.Dapper.SqlServer",

    [string] $AssemblyName = "Dapper.TypedParameters.SqlServer",

    [string] $RepositoryUrl = "https://github.com/rodri-oliveira-dev/Dapper.TypedParameters"
)

$ErrorActionPreference = "Stop"

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
    param([string[]] $Entries)

    $forbidden = @(
        '(^|/)bin/',
        '(^|/)obj/',
        '\.tmp$',
        '\.temp$',
        '\.user$',
        '\.suo$',
        'Dapper\.TypedParameters\.SqlServer\.Tests\.dll$',
        'Dapper\.TypedParameters\.SqlServer\.IntegrationTests\.dll$'
    )

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

$packagePath = Get-ChildItem -LiteralPath $PackageDirectory -Filter "$PackageId*.nupkg" |
    Where-Object { $_.Name -notlike '*.symbols.nupkg' } |
    Select-Object -First 1
$symbolPath = Get-ChildItem -LiteralPath $PackageDirectory -Filter "$PackageId*.snupkg" |
    Select-Object -First 1

Assert-True ($null -ne $packagePath) "No .nupkg found in $PackageDirectory."
Assert-True ($null -ne $symbolPath) "No .snupkg found in $PackageDirectory."

$packageRoot = Expand-Package $packagePath.FullName
$symbolRoot = Expand-Package $symbolPath.FullName

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
    Test-NoForbiddenEntries $entries
    Test-NoForbiddenEntries $symbolEntries
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
    Assert-True ($metadata.repository.url -eq $RepositoryUrl) "Repository URL metadata was not found."

    $dependencyIds = @($metadata.dependencies.group.dependency | ForEach-Object { $_.id })
    Assert-True ($dependencyIds -contains 'Dapper') "Dapper dependency metadata was not found."
    Assert-True ($dependencyIds -contains 'Microsoft.Data.SqlClient') "Microsoft.Data.SqlClient dependency metadata was not found."

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
