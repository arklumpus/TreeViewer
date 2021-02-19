ext=$1

cd $ext
mkdir $ext.iconset

cp $ext-16.png $ext.iconset/icon_16x16.png
cp $ext-32.png $ext.iconset/icon_16x16@2x.png
cp $ext-32.png $ext.iconset/icon_32x32.png
cp $ext-64.png $ext.iconset/icon_32x32@2x.png
cp $ext-128.png $ext.iconset/icon_128x128.png
cp $ext-256.png $ext.iconset/icon_128x128@2x.png
cp $ext-256.png $ext.iconset/icon_256x256.png
cp $ext-512.png $ext.iconset/icon_256x256@2x.png
cp $ext-512.png $ext.iconset/icon_512x512.png
cp $ext-1024.png $ext.iconset/icon_512x512@2x.png

cd ..

