@echo off

if "%~1"=="" (
	echo.
	echo [91mThis tool requires the following parameters:[0m
	echo [91m    Path to P12/PFX certificate file[0m
	echo [91m    Certificate password[0m
	
	exit /b 64
)

if "%~2"=="" (
	echo.
	echo [91mThis tool requires the following parameters:[0m
	echo [91m    Path to P12/PFX certificate file[0m
	echo [91m    Certificate password[0m
	
	exit /b 64
)

if not exist %1 (
	echo.
	echo [91mThe specified certificate file does not exist![0m
	
	exit /b 1
)

echo.
echo Creating binary files for [94mWin-x64[0m

mkdir Binary\Win-x64

echo.
echo [104;97mDeleting previous build...[0m

for /f %%i in ('dir /a:d /b Binary\Win-x64\*') do rd /s /q Binary\Win-x64\%%i
del Binary\Win-x64\* /s /f /q 1>nul

echo.
echo [104;97mSigning files...[0m

echo.
echo [94mSigning executable files[0m

signtool sign /fd sha256 /f %1 /p %2 /tr "http://ts.ssl.com" /td sha256 /v /a /d "TreeViewer: cross-platform software to draw phylogenetic trees" /du "https://github.com/arklumpus/TreeViewer" Release\Win-x64\*.exe

echo.
echo [94mSigning DLLs[0m

signtool sign /fd sha256 /f %1 /p %2 /tr "http://ts.ssl.com" /td sha256 /v /a /d "TreeViewer: cross-platform software to draw phylogenetic trees" /du "https://github.com/arklumpus/TreeViewer" Release\Win-x64\*.dll

for /f %%i in ('dir /a:d /b Release\Win-x64\*') do signtool sign /fd sha256 /f %1 /p %2 /tr "http://ts.ssl.com" /td sha256 /v /a /d "TreeViewer: cross-platform software to draw phylogenetic trees" /du "https://github.com/arklumpus/TreeViewer" Release\Win-x64\%%i\*.dll

echo.
echo [104;97mCreating MSI installer...[0m

cd Installers\Win-x64
call make.cmd

echo.
echo [94mSigning installer[0m

signtool sign /fd sha256 /f %1 /p %2 /tr "http://ts.ssl.com" /td sha256 /v /a /d "TreeViewer: cross-platform software to draw phylogenetic trees" /du "https://github.com/arklumpus/TreeViewer" TreeViewer-Win-x64.msi

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
