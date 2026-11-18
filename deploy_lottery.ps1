$src = "D:\VibeCode\Probably Stolen\custom_mods\LotteryForesight"
$dst = "D:\GameCrack\Probably Stolen\Mods\LotteryForesight"
Copy-Item "$src\LotteryForesight.dll" "$dst\LotteryForesight.dll" -Force
Copy-Item "$src\manifest.xml" "$dst\manifest.xml" -Force
Get-ChildItem $dst | Select-Object Name, Length, LastWriteTime
