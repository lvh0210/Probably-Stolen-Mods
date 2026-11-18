$managed = "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed"
$mono = "D:\Unity\Editor\2022.3.62f1\Editor\Data\MonoBleedingEdge\bin\mono.exe"
$csc = "D:\Unity\Editor\2022.3.62f1\Editor\Data\MonoBleedingEdge\lib\mono\4.5\csc.exe"
$src = "D:\VibeCode\Probably Stolen\custom_mods\SmartInventorySort\SmartInventorySortMod.cs"
$out = "D:\VibeCode\Probably Stolen\custom_mods\SmartInventorySort\SmartInventorySort.dll"

$refs = @(
    "$managed\mscorlib.dll",
    "$managed\netstandard.dll",
    "$managed\Assembly-CSharp.dll",
    "$managed\0Harmony.dll",
    "$managed\UnityEngine.dll",
    "$managed\UnityEngine.CoreModule.dll"
)

$refArgs = $refs | ForEach-Object { "/reference:`"$_`"" }

& $mono $csc /target:library /nostdlib+ /langversion:latest "/out:$out" $refArgs $src
Write-Host "ExitCode: $LASTEXITCODE"
