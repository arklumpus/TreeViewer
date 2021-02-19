@echo off

del TreeViewer.wxs
del TreeViewer.wixobj
del TreeViewer-win-x64.msi

for /f %%i in ('dir /a:d /b SourceDir\*') do rd /s /q SourceDir\%%i
del SourceDir\* /s /f /q 1>nul

xcopy ..\..\Release\Win-x64 SourceDir\ /s /y /h

del SourceDir\*.pdb

"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csi.exe" GenerateFileGuids.csx

candle TreeViewer.wxs

light -ext WixUIExtension TreeViewer.wixobj

ren TreeViewer.msi TreeViewer-Win-x64.msi
