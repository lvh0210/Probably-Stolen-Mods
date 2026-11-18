$targetDir = "D:\GameCrack\Probably Stolen\Mods\SmartInventorySort"
if (!(Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir | Out-Null }
Copy-Item "D:\VibeCode\Probably Stolen\custom_mods\SmartInventorySort\SmartInventorySort.dll" $targetDir -Force
Copy-Item "D:\VibeCode\Probably Stolen\custom_mods\SmartInventorySort\manifest.xml" $targetDir -Force
Get-ChildItem $targetDir
