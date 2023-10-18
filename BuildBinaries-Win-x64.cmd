@echo off
setlocal EnableDelayedExpansion

if "%~1"=="" (
	echo.
	echo [91mThis tool requires the following parameters:[0m
	echo [91m    Code signing certificate subject name[0m
	echo [91m    Smart card PIN[0m
	
	exit /b 64
)

if "%~2"=="" (
	echo.
	echo [91mThis tool requires the following parameters:[0m
	echo [91m    Code signing certificate subject name[0m
	echo [91m    Smart card PIN[0m
	
	exit /b 64
)

set certsub=%1
set certpin=%2

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
echo [94mBuilding list of files to sign...[0m

set n=0

for /f %%i in ('dir /b Release\Win-x64\*.exe') do set fileList[!n!]=Release\Win-x64\%%i && set /A n+=1
for /f %%i in ('dir /b Release\Win-x64\*.dll') do set fileList[!n!]=Release\Win-x64\%%i && set /A n+=1
for /f %%i in ('dir /a:d /b Release\Win-x64\*') do for /f %%j in ('dir /b Release\Win-x64\%%i\*.dll') do set fileList[!n!]=Release\Win-x64\%%i\%%j && set /A n+=1

echo.
echo !n! files need to be signed.

set /A max=!n!-1

set /A c=(!n!+5)/10

for /l %%i in (0,10,!n!) do (
	
	set /A ci=%%i/10 + 1
	
	echo.
	echo [94mSigning batch !ci! out of !c![0m
	
	set /A end=%%i+9
	
	if !end! gtr !max! set end=!max!
	
	set currcmd=

	for /l %%j in (%%i,1,!end!) do set currcmd=!currcmd! !fileList[%%j]!
	
	call :sign
	
	signtool verify /pa !currcmd! || exit /b
	
	echo.
)

echo.
echo [104;97mAll files signed and verified![0m

echo.
echo [104;97mCreating MSI installer...[0m

cd Installers\Win-x64
call make.cmd || exit /b

echo.
echo [94mSigning installer[0m

set currcmd=TreeViewer-Win-x64.msi

call :sign

signtool verify /pa !currcmd! || exit /b

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

exit /b

:sign

goto start

:retry
echo.
echo [91mRetrying[0m
echo.

:start

scsigntool -pin %certpin% sign /fd sha256 /n %certsub% /tr "http://ts.ssl.com" /td sha256 /v /a /d "TreeViewer: cross-platform software to draw phylogenetic trees" /du "https://treeviewer.org" !currcmd! || goto retry

exit /B