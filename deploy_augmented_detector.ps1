$srcDir = "D:\VibeCode\Probably Stolen\custom_mods\AugmentedDetector"
$dstDir = "D:\GameCrack\Probably Stolen\Mods\AugmentedDetector"

if (-not (Test-Path $dstDir)) {
    New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
}

Copy-Item "$srcDir\AugmentedDetector.dll" "$dstDir\AugmentedDetector.dll" -Force
Copy-Item "$srcDir\manifest.xml" "$dstDir\manifest.xml" -Force

Write-Host "Deployed AugmentedDetector to ${dstDir}"
Get-ChildItem $dstDir | Select-Object Name, Length
