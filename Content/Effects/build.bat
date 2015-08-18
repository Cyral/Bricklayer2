@echo off
REM - This script will build the .fx file into a compiled MonoGame Effect file using 2MGFX.exe. DirectX must be installed.
:A
for %%f in (*.fx) do (
"C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe" blur.fx blur.mgfxo /Profile:DirectX_11
)
pause
goto A