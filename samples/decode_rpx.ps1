$inputFile = Join-Path $PSScriptRoot "rpx.js"
$outputDir = Join-Path $PSScriptRoot "rpx_folder"

# Create output directory if it doesn't exist
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$data = Get-Content $inputFile -Raw | ConvertFrom-Json

foreach ($prop in $data.PSObject.Properties) {
    $filename = $prop.Name
    $entry    = $prop.Value

    if ($entry.base64) {
        $bytes      = [Convert]::FromBase64String($entry.base64)
        $outputPath = Join-Path $outputDir $filename
        [IO.File]::WriteAllBytes($outputPath, $bytes)
        Write-Host "Decoded: $filename -> rpx_folder\$filename"
    } else {
        Write-Warning "Skipped: $filename (no base64 field)"
    }
}

Write-Host "Done."
