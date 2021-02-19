@echo off

echo.
echo Creating binary files for [94mWin-x64[0m

mkdir Binary\Win-x64

echo.
echo [104;97mDeleting previous build...[0m

for /f %%i in ('dir /a:d /b Binary\Win-x64\*') do rd /s /q Binary\Win-x64\%%i
del Binary\Win-x64\* /s /f /q 1>nul

echo.
echo [104;97mCreating MSI installer...[0m

cd Installers\Win-x64
call make.cmd

echo.
echo [104;97mCreating ZIP file...[0m

move SourceDir TreeViewer-Win-x64
zip -r TreeViewer-Win-x64.zip TreeViewer-Win-x64
move TreeViewer-Win-x64 SourceDir

for /f %%i in ('dir /a:d /b SourceDir\*') do rd /s /q SourceDir\%%i
del SourceDir\* /s /f /q 1>nul

cd ..\..

move Installers\Win-x64\TreeViewer-Win-x64.msi Binary\Win-x64\
move Installers\Win-x64\TreeViewer-Win-x64.zip Binary\Win-x64\

echo.
echo [94mAll done![0m
