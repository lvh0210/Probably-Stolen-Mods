[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -Path "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed\DseCecil.dll"
$dll = "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed\Assembly-CSharp.dll"
$asm = [DseCecil.AssemblyDefinition]::ReadAssembly($dll)

Write-Host "=== Search for Redeem ==="
foreach ($t in $asm.MainModule.Types) {
    foreach ($m in $t.Methods) {
        if ($m.Name -like "*Redeem*" -or $m.Name -like "*Scratch*") {
            Write-Host "$($t.FullName)::$($m.Name)"
        }
    }
}
