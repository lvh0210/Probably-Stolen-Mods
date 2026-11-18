$src = "D:\GameCrack\Probably Stolen\Mods\baishe_all_characters_feminized\Bai She's All Original Characters Feminized 1.0.0-Beta.dll"
$bak = "D:\GameCrack\Probably Stolen\Mods\baishe_all_characters_feminized\Bai She's All Original Characters Feminized 1.0.0-Beta.dll.bak"
Copy-Item $src $bak -Force
Write-Host "Backup created: $(Test-Path $bak)"
