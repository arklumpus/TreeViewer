#!/usr/bin/env zsh

version=$(strings ../../Release/Mac-arm64/TreeViewer.app/Contents/MacOs/TreeViewer.dll | grep -A3 "Cross-platform software to draw phylogenetic trees" | grep -v "TreeViewer" | tail -n1)

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

cd ../../Release/Mac-arm64

pkgbuild --install-location /Applications --component "TreeViewer.app" "TreeViewer_signed.pkg"

mv "TreeViewer_signed.pkg" ../../Installers/Mac-arm64/

cd ../../Installers/Mac-arm64/

pkgutil --expand "TreeViewer_signed.pkg" "TreeViewer_signedPKG"

pkgutil --expand "TreeViewer.pkg" "TreeViewerPKG"

rm "TreeViewerPKG/TreeViewer.pkg/Payload"

mv "TreeViewer_signedPKG/Payload" "TreeViewerPKG/TreeViewer.pkg/"

rm "TreeViewer.pkg" "TreeViewer_signed.pkg"
rm -r "TreeViewer_signedPKG"

pkgutil --flatten "TreeViewerPKG" "TreeViewer.pkg"

rm -r "TreeViewerPKG"

echo
echo -e "\033[104m\033[97m Signing PKG \033[0m"
echo

productsign --sign "$1" "TreeViewer.pkg" "TreeViewer_signed.pkg"

if [ -f "TreeViewer_signed.pkg" ]; then
	mv "TreeViewer_signed.pkg" "TreeViewer.pkg"
fi

pkgutil --check-signature "TreeViewer.pkg"

echo
echo -e "\033[104m\033[97m Notarizing PKG \033[0m"
echo

xcrun notarytool submit TreeViewer.pkg --apple-id "$2" --password "$3" --team-id "$4" --wait
xcrun stapler staple TreeViewer.pkg
xcrun stapler validate TreeViewer.pkg

echo
echo -e "\033[94mAll done!\033[0m"
echo