@echo off
echo Cleaning solution...

rem Delete bin and obj folders
for /d /r . %%d in (bin obj) do @if exist "%%d" rd /s/q "%%d"

echo Cleaning complete. Please rebuild the solution in Visual Studio.
pause 