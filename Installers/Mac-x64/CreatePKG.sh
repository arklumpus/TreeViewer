#!/usr/bin/env zsh

version=$(strings ../../Release/Mac-x64/TreeViewer.app/Contents/Resources/TreeViewer.app/Contents/MacOs/TreeViewer.dll | grep -A3 "TreeViewer" | grep -v "TreeViewer" | grep -A2 "Release" | tail -n1)

echo
echo -e "\033[104m\033[97m Setting version $version \033[0m"
echo

rm -f TreeViewer.pkgproj

cp TreeViewer.pkgproj.original TreeViewer.pkgproj

sed -i '' "s/@@VersionHere@@/$version/g" TreeViewer.pkgproj

echo
echo -e "\033[104m\033[97m Creating PKG \033[0m"
echo

packagesbuild TreeViewer.pkgproj

rm TreeViewer.pkgproj

echo
echo -e "\033[104m\033[97m Signing PKG \033[0m"
echo

codesign --deep --force --timestamp --sign "$1" "TreeViewer.pkg"

codesign --verify --verbose "TreeViewer.pkg"

echo
echo -e "\033[94mAll done!\033[0m"
echo