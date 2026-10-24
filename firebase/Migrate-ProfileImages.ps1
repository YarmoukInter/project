param(
    [Parameter(Mandatory=$true)][string]$SourceDirectory,
    [Parameter(Mandatory=$true)][string]$DestinationDirectory
)
$ErrorActionPreference = 'Stop'
$source = (Resolve-Path -LiteralPath $SourceDirectory).Path
if (-not (Test-Path -LiteralPath $source -PathType Container)) { throw 'Source must be an existing directory.' }
$destination = [IO.Path]::GetFullPath($DestinationDirectory)
if ($source.TrimEnd('\') -eq $destination.TrimEnd('\')) { throw 'Choose a separate persistent destination directory.' }
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$copied = 0
Get-ChildItem -LiteralPath $source -File | Where-Object { $_.Extension -in '.png','.jpg','.jpeg' } | ForEach-Object {
    $target = Join-Path $destination $_.Name
    if (Test-Path -LiteralPath $target) {
        if ((Get-FileHash -LiteralPath $_.FullName).Hash -ne (Get-FileHash -LiteralPath $target).Hash) {
            throw "An image with a different checksum already exists: $($_.Name)"
        }
    } else {
        Copy-Item -LiteralPath $_.FullName -Destination $target
        $copied++
    }
}
Write-Output "Copied $copied images. Originals and matching existing images were preserved."
