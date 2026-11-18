$managed = "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$src = "D:\VibeCode\Probably Stolen\custom_mods\LotteryForesight\LotteryForesightMod.cs"
$out = "D:\VibeCode\Probably Stolen\custom_mods\LotteryForesight\LotteryForesight.dll"

$refs = @(
    "$managed\mscorlib.dll",
    "$managed\netstandard.dll",
    "$managed\Assembly-CSharp.dll",
    "$managed\0Harmony.dll",
    "$managed\UnityEngine.dll",
    "$managed\UnityEngine.CoreModule.dll",
    "$managed\UnityEngine.InputLegacyModule.dll",
    "$managed\UnityEngine.UI.dll"
)

$refArgs = $refs | ForEach-Object { "/reference:`"$_`"" }

& $csc /target:library /nostdlib+ "/out:$out" $refArgs $src
if ($LASTEXITCODE -eq 0) {
    Write-Host "COMPILED SUCCESSFULLY: $out"
} else {
    Write-Host "COMPILATION FAILED with code $LASTEXITCODE"
}
