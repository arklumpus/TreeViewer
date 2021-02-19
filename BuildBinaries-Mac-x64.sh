#!/usr/bin/env zsh

if [ "$#" -ne 1 ]; then
    echo "This script requires the signing identity as a parameter"
    exit 64
fi

rm -rf Binary/Mac-x64

mkdir -p Binary/Mac-x64

cd Installers/Mac-x64/

./CreateDMG.sh $1

cd ../..

mv TreeViewer.dmg Binary/Mac-x64/TreeViewer-Mac-x64.dmg

cd Installers/Mac-x64

./CreatePKG.sh $1

mv TreeViewer.pkg ../../Binary/Mac-x64/TreeViewer-Mac-x64.pkg

cd ../..

