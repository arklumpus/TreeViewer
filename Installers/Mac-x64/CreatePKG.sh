#!/usr/bin/env zsh

version=$(strings ../../Release/Mac-x64/TreeViewer.app/Contents/Resources/TreeViewer.app/Contents/MacOs/TreeViewer.dll | grep -A3 "Cross-platform software to draw phylogenetic trees" | grep -v "TreeViewer" | tail -n1)

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

cd ../../Release/Mac-x64

pkgbuild --install-location /Applications --component "TreeViewer.app" "TreeViewer_signed.pkg"

mv "TreeViewer_signed.pkg" ../../Installers/Mac-x64/

cd ../../Installers/Mac-x64/

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

requestID=$(xcrun altool --notarize-app -f "TreeViewer.pkg" --primary-bundle-id "io.github.arklumpus.TreeViewer" -u "$2" -p "$3" | grep "RequestUUID" | cut -d" " -f 3)

echo "Request UUID: $requestID"

breakloop="0"

while [ $breakloop -lt 1 ]; do

    echo "Waiting for 1 minute..."
    sleep 60

    currStatus=$(xcrun altool --notarization-info $requestID -u $2 -p $3 | grep "Status:" | cut -d":" -f 2)

    echo "Status: $currStatus"

    if [ "$currStatus" != " in progress" ]; then
    	if [ "$currStatus" = " success" ]; then
    	    breakloop="2"
    	else
    	    breakloop="1"
    	fi
    fi

done

if [ $breakloop -eq 2 ]; then

    echo
    echo -e "\033[104m\033[97m Stapling PKG \033[0m"
    echo

	xcrun stapler staple TreeViewer.pkg
	xcrun stapler validate TreeViewer.pkg

else

    echo
    echo -e "\033[101m\033[97m PKG notarization failed! \033[0m"
    echo

fi

echo
echo -e "\033[94mAll done!\033[0m"
echo