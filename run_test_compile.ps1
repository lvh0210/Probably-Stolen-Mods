$managed = "D:\GameCrack\Probably Stolen\Probably Stolen_Data\Managed"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /target:library /nostdlib+ "/reference:$managed\mscorlib.dll" "/reference:$managed\netstandard.dll" "/reference:$managed\Assembly-CSharp.dll" "D:\VibeCode\Probably Stolen\test_gridshape.cs"
