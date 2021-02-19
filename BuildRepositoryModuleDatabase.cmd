@echo off

echo.
echo [104;97mDeleting previous repository...[0m

for /f %%i in ('dir /a:d /b Modules\*') do rd /s /q Modules\%%i
del Modules\* /s /f /q 1>nul

echo.
echo [104;97mBuilding BuildRepositoryModuleDatabase...[0m

cd src\BuildRepositoryModuleDatabase
dotnet publish -c Release /p:PublishProfile=Properties\PublishProfiles\Win-x64.pubxml
cd ..\..

echo.
echo [104;97mRunning BuildRepositoryModuleDatabase...[0m

cd Release\Win-x64
BuildRepositoryModuleDatabase --root ..\.. --key %1
cd ..\..

echo.
echo [104;97mCreating repository archive...[0m

bash -c "tar -czf modules.tar.gz Modules"
move modules.tar.gz Modules/modules.tar.gz

echo.
echo [94mAll done![0m