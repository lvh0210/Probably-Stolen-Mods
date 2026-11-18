[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -Path "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed\DseCecil.dll"
$dll = "D:\GameCrack\Probably Stolen\Mods\baishe_all_characters_feminized\Bai She's All Original Characters Feminized 1.0.0-Beta.dll"
$asm = [DseCecil.AssemblyDefinition]::ReadAssembly($dll)

function Show-Instructions($typeFullName, $methodName) {
    Write-Host "=== $typeFullName :: $methodName ==="
    $type = $asm.MainModule.Types | Where-Object { $_.FullName -eq $typeFullName }
    $m = $type.Methods | Where-Object { $_.Name -eq $methodName }
    for ($i = 0; $i -lt $m.Body.Instructions.Count; $i++) {
        $instr = $m.Body.Instructions[$i]
        Write-Host ("[{0}] {1:X4} {2} {3}" -f $i, $instr.Offset, $instr.OpCode.Name, $instr.Operand)
    }
}

Show-Instructions "AugAnomalyTrial.EffectPolicy" "Bind"
Show-Instructions "AugAnomalyTrial.EffectPolicy" "Tick"
