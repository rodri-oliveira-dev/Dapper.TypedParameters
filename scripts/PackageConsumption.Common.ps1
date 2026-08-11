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
