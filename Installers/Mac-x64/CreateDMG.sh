#!/usr/bin/env bash

echo
echo -e "\033[104m\033[97m Adding executable flags \033[0m"
echo

cd ../../Release/Mac-x64/TreeViewer.app/Contents/MacOs/

chmod +x "Automator Application Stub"

cd ../Resources/TreeViewer.app/Contents/MacOs/

chmod +x TreeViewer TreeViewerCommandLine DebuggerClient

version=$(strings TreeViewer.dll | grep -A3 "Cross-platform software to draw phylogenetic trees" | grep -v "TreeViewer" | tail -n1)

echo -e "\033[104m\033[97m Setting version number $version \033[0m"
echo

sed -i '' "s/@@VersionHere@@/$version/g" ../../../../Info.plist
sed -i '' "s/@@VersionHere@@/$version/g" ../Info.plist
sed -i '' "s/@@VersionHere@@/$version/g" ../Resources/DebuggerClient.app/Contents/Info.plist

echo -e "\033[104m\033[97m Creating symlinks for DebuggerClient \033[0m"
echo

rm -rf ../Resources/DebuggerClient.app/Contents/MacOs/

mkdir ../Resources/DebuggerClient.app/Contents/MacOs/

for i in *; do
	ln -s "../../../../MacOs/$i" "../Resources/DebuggerClient.app/Contents/MacOs/$i"
done

cd ../../../../../../

echo -e "\033[104m\033[97m Signing app \033[0m"
echo

codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app"

codesign --verify --verbose "TreeViewer.app"

cd ../..

echo
echo -e "\033[104m\033[97m Creating DMG \033[0m"
echo

hdiutil create -srcfolder Release/Mac-x64 -volname "TreeViewer" -fs HFS+ -format UDRW -size 300m "TreeViewer.rw.dmg"

device=$(hdiutil attach -readwrite -noverify -noautoopen "TreeViewer.rw.dmg" | grep -e "^/dev/" | head -n1 | cut -f 1)

rm /Volumes/TreeViewer/TreeViewer.entitlements

mkdir /Volumes/TreeViewer/.background/

cp Icons/Installers/DMG/DMGBackground.png /Volumes/TreeViewer/.background/background.png

echo '
tell application "Finder"
 tell disk "TreeViewer"
  open
  set current view of container window to icon view
  set toolbar visible of container window to false
  set statusbar visible of container window to false
  set the bounds of container window to {100, 100, 795, 660}
  set iconViewOptions to the icon view options of container window
  set arrangement of iconViewOptions to not arranged
  set background picture of iconViewOptions to file ".background:background.png"
  set icon size of iconViewOptions to 128
  set position of item "TreeViewer.app" of container window to {140, 120}
  make new alias file at container window to POSIX file "/Applications" with properties {name:"Applications"}
  set position of item "Applications" of container window to {555, 120}
  close
  open
  update without registering applications
  close
 end tell
end tell' | osascript

cp Icons/Installers/DMG/DMGIcon.icns /Volumes/TreeViewer/.VolumeIcon.icns
SetFile -a C /Volumes/TreeViewer

sync
sync

hdiutil detach ${device}

hdiutil convert "TreeViewer.rw.dmg" -format UDZO -o "TreeViewer.dmg"

rm "TreeViewer.rw.dmg"

echo
echo -e "\033[104m\033[97m Signing DMG \033[0m"
echo

codesign --deep --force --timestamp --sign "$1" "TreeViewer.dmg"

codesign --verify --verbose "TreeViewer.dmg"

echo
echo -e "\033[94mAll done!\033[0m"
echo



