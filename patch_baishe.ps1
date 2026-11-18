[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -Path "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed\DseCecil.dll"
$dll = "D:\GameCrack\Probably Stolen\Mods\baishe_all_characters_feminized\Bai She's All Original Characters Feminized 1.0.0-Beta.dll"

$bytes = [System.IO.File]::ReadAllBytes($dll)
$ms = [System.IO.MemoryStream]::new($bytes)
$asm = [DseCecil.AssemblyDefinition]::ReadAssembly($ms)

# 1. Patch AugAnomalyTrial.EffectPolicy::Bind
$epType = $asm.MainModule.Types | Where-Object { $_.FullName -eq "AugAnomalyTrial.EffectPolicy" }
$bindMethod = $epType.Methods | Where-Object { $_.Name -eq "Bind" }

$bindMethod.Body.Instructions[12].OpCode = [DseCecil.Cil.OpCodes]::Ldc_I4_0
$bindMethod.Body.Instructions[12].Operand = $null
$bindMethod.Body.Instructions[15].OpCode = [DseCecil.Cil.OpCodes]::Ldc_I4_0
$bindMethod.Body.Instructions[15].Operand = $null
Write-Host "Patched EffectPolicy.Bind (100% Obvious on isAug)"

# 2. Patch AugAnomalyTrial.EffectPolicy::Tick
$tickMethod = $epType.Methods | Where-Object { $_.Name -eq "Tick" }
$tickMethod.Body.Instructions[31].OpCode = [DseCecil.Cil.OpCodes]::Ldc_I4
$tickMethod.Body.Instructions[31].Operand = [int]1000000
$tickMethod.Body.Instructions[37].Operand = [double]2.0
$tickMethod.Body.Instructions[55].Operand = [double]3.5
Write-Host "Patched EffectPolicy.Tick (continuous pulsing every 3.5s)"

# 3. Patch AugAnomalyTrial.AnomalyEffects::RenderGhost
$aeType = $asm.MainModule.Types | Where-Object { $_.FullName -eq "AugAnomalyTrial.AnomalyEffects" }
$rgMethod = $aeType.Methods | Where-Object { $_.Name -eq "RenderGhost" }
$rgMethod.Body.Instructions[94].Operand = [single]0.10
$rgMethod.Body.Instructions[163].Operand = [single]0.85
Write-Host "Patched AnomalyEffects.RenderGhost (amplitude 0.10, alpha 0.85)"

$tempDll = $dll + ".tmp"
$asm.Write($tempDll)
Move-Item $tempDll $dll -Force
Write-Host "SUCCESSFULLY SAVED PATCHED BAISHE MOD DLL!"
