param(
    [string]$PluginRoot =
        "C:\JellyInspector\Application\src\Jellyfin.Plugin.JellyInspector"
)

$ErrorActionPreference = "Stop"

$CssRoot = Join-Path $PluginRoot "Web\css"
$Output = Join-Path $CssRoot "jellyinspector.css"

$Sources = @(
    "variables.css",
    "layout.css",
    "cards.css",
    "components.css",
    "animations.css",
    "responsive.css"
)

$Builder = New-Object System.Text.StringBuilder

foreach ($Source in $Sources) {
    $Path = Join-Path $CssRoot $Source

    if (-not (Test-Path $Path)) {
        throw "No existe el archivo CSS: $Path"
    }

    [void]$Builder.AppendLine(
        "/* ========================================")

    [void]$Builder.AppendLine(
        "   $Source")

    [void]$Builder.AppendLine(
        "   ======================================== */")

    [void]$Builder.AppendLine(
        (Get-Content $Path -Raw))

    [void]$Builder.AppendLine()
}

[System.IO.File]::WriteAllText(
    $Output,
    $Builder.ToString(),
    [System.Text.UTF8Encoding]::new($false))

Write-Host ""
Write-Host "CSS de JellyInspector generado:"
Write-Host $Output
