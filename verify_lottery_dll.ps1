[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -Path "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed\DseCecil.dll"
$dll = "D:\GameCrack\Probably Stolen\Mods\LotteryForesight\LotteryForesight.dll"
$asm = [DseCecil.AssemblyDefinition]::ReadAssembly($dll)
Write-Host "Types in LotteryForesight.dll:"
foreach ($t in $asm.MainModule.Types) {
    Write-Host "  Type: $($t.FullName)"
    foreach ($m in $t.Methods) {
        Write-Host "    Method: $($m.Name)"
    }
}
