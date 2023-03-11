#!/usr/bin/env zsh

if [ "$#" -ne 5 ]; then
    echo "This script requires the following parameters:"
    echo "    Developer ID Application signing identity"
    echo "    Developer ID Installer signing identity"
    echo "    Apple ID (email address)"
    echo "    App-specific password for the Apple ID"
    echo "    Developer team ID"
    exit 64
fi

rm -rf Binary/Mac-arm64

mkdir -p Binary/Mac-arm64

cd Installers/Mac-arm64/

./CreateDMG.sh $1 $3 $4 $5

cd ../..

mv TreeViewer.dmg Binary/Mac-arm64/TreeViewer-Mac-arm64.dmg

cd Installers/Mac-arm64

./CreatePKG.sh $2 $3 $4 $5

mv TreeViewer.pkg ../../Binary/Mac-arm64/TreeViewer-Mac-arm64.pkg

cd ../..

