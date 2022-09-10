@echo off

echo.

set platform=%1

set found=0

if "%platform%" == "Linux-x64" set found=1
if "%platform%" == "Win-x64" set found=1
if "%platform%" == "Mac-x64" set found=1
if "%platform%" == "Mac-arm64" set found=1

if %found% == 0 (
	echo [91mInvalid platform specified![0m Valid options are: [94mLinux-64[0m, [94mWin-x64[0m, [94mMac-x64[0m, or [94mMac-arm64[0m
	exit /B 64
)

echo Building with target [94m%1[0m


echo.
echo [104;97mDeleting previous build...[0m

for /f %%i in ('dir /a:d /b Release\%1\*') do rd /s /q Release\%1\%%i
del Release\%1\* /s /f /q 1>nul

echo.
echo [104;97mCopying common resources...[0m

xcopy src\Resources\%1 Release\%1\ /s /y /h

if "%platform%" == "Win-x64" (
echo.
echo [104;97mBuilding Stairs...[0m

cd src\Stairs
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..\..
)

if "%platform%" == "Win-x64" (
echo.
echo [104;97mBuilding Elevator...[0m

cd src\Elevator
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..\..
)

echo.
echo [104;97mBuilding DebuggerClient...[0m

cd src\DebuggerClient
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..\..

echo.
echo [104;97mBuilding TreeViewerCommandLine...[0m

cd src\TreeViewerCommandLine
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..\..

echo.
echo [104;97mBuilding TreeViewer...[0m

cd src\TreeViewer
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..\..

echo.
echo [94mAll done![0m