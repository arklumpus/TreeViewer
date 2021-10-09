rm -rf Binary/Linux-x64

mkdir -p Binary/Linux-x64

cd Installers/Linux-x64

./make.sh

mv TreeViewer-Linux-x64.run ../../Binary/Linux-x64/
mv TreeViewer-Linux-x64.tar.gz ../../Binary/Linux-x64/

cd ../..
