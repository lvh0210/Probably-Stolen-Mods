[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -Path "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed\DseCecil.dll"
$dll = "D:\GameCrack\Probably Stolen\Mods\baishe_all_characters_feminized\Bai She's All Original Characters Feminized 1.0.0-Beta.dll"
$asm = [DseCecil.AssemblyDefinition]::ReadAssembly($dll)
$t = $asm.MainModule.Types | Where-Object { $_.FullName -eq "AugAnomalyTrial.AnomalyEffects" }
$m = $t.Methods | Where-Object { $_.Name -eq "RenderGhost" }
for ($i = 160; $i -lt [Math]::Min($m.Body.Instructions.Count, 200); $i++) {
    $instr = $m.Body.Instructions[$i]
    Write-Host ("[{0}] {1:X4} {2} {3}" -f $i, $instr.Offset, $instr.OpCode.Name, $instr.Operand)
}
