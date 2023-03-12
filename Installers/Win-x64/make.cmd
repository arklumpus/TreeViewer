@echo off

del TreeViewer.wxs
del TreeViewer.wixobj
del TreeViewer-win-x64.msi

for /f %%i in ('dir /a:d /b SourceDir\*') do rd /s /q SourceDir\%%i
del SourceDir\* /s /f /q 1>nul

xcopy ..\..\Release\Win-x64 SourceDir\ /s /y /h

del SourceDir\*.pdb

csi GenerateFileGuids.csx

candle TreeViewer.wxs -ext WixUtilExtension

light -ext WixUIExtension -ext WixUtilExtension TreeViewer.wixobj

ren TreeViewer.msi TreeViewer-Win-x64.msi
