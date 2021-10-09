#!/bin/bash

rm -rf TreeViewer_setup/*
mkdir -p TreeViewer_setup/TreeViewer-Linux-x64

cp -r ../../Release/Linux-x64/* TreeViewer_setup/TreeViewer-Linux-x64/

rm TreeViewer_setup/TreeViewer-Linux-x64/*.pdb

cd TreeViewer_setup

tar -czf TreeViewer-Linux-x64.tar.gz TreeViewer-Linux-x64/

mv TreeViewer-Linux-x64.tar.gz ../

cd ..

cp TreeViewer_setup.sh TreeViewer_setup/

makeself-2.4.0/makeself.sh TreeViewer_setup TreeViewer-Linux-x64.run "TreeViewer" ./TreeViewer_setup.sh

rm -rf TreeViewer_setup/*