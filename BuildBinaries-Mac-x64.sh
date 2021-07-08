#!/usr/bin/env zsh

if [ "$#" -ne 4 ]; then
    echo "This script requires the following parameters:"
    echo "    Developer ID Application signing identity"
    echo "    Developer ID Installer signing identity"
    echo "    Apple ID (email address)"
    echo "    App-specific password for the Apple ID"
    exit 64
fi

rm -rf Binary/Mac-x64

mkdir -p Binary/Mac-x64

cd Installers/Mac-x64/

./CreateDMG.sh $1 $3 $4

cd ../..

mv TreeViewer.dmg Binary/Mac-x64/TreeViewer-Mac-x64.dmg

cd Installers/Mac-x64

./CreatePKG.sh $2 $3 $4

mv TreeViewer.pkg ../../Binary/Mac-x64/TreeViewer-Mac-x64.pkg

cd ../..

