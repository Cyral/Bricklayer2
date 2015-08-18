@echo off
:A
"C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe" blur.fx blur.mgfxo /Profile:DirectX_11
copy blur.mgfxo "W:\Bricklayer\Source\Core\Client\bin\Windows\Debug\Content\Effects\blur.mgfxo"
pause
goto A