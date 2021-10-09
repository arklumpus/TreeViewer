#!/bin/sh

printf "\nWelcome to the TreeViewer setup!\n\n"

#Check that we are running as root
if [ $(id -u) -gt 0 ]; then
    printf "This script needs to be run as root!\n\n"
    exit 1
fi

printf "This script will copy the program files, add TreeViewer to the desktop\nmenu, and (optionally) symlink the TreeViewer executables so that they\ncan be recalled from anywhere.\n\n"

printf "\n"

prefix="/usr/lib"

printf "\nThe TreeViewer program files will now be copied.\n\nThe default location is: $prefix/TreeViewer\n"

confirm="a"

while [ "$confirm" != "Y" ] && [ "$confirm" != "y" ] && [ "$confirm" != "N" ] && [ "$confirm" != "n" ]; do
    read -p "Do you wish to install TreeViewer in the default location? [Y/n] " confirm
done

if [ "$confirm" != "Y" ] && [ "$confirm" != "y" ]; then
    read -p "Enter new install location: " prefix
fi

printf "\nTreeViewer will be installed in: $prefix/TreeViewer\n"

confirm="a"

while [ "$confirm" != "Y" ] && [ "$confirm" != "y" ] && [ "$confirm" != "N" ] && [ "$confirm" != "n" ]; do
    read -p "Do you wish to continue? [Y/n] " confirm
done

if [ "$confirm" != "Y" ] && [ "$confirm" != "y" ]; then
    printf "\nAborted.\n\n"
    exit 0
fi

rm -rf "${prefix}/TreeViewer"
mv TreeViewer-Linux-x64 "${prefix}/TreeViewer"
chmod +x "${prefix}/TreeViewer/TreeViewer" "${prefix}/TreeViewer/TreeViewerCommandLine" "${prefix}/TreeViewer/DebuggerClient"
sed -i "s;@PATHHERE@;$prefix/TreeViewer/TreeViewer;g" "${prefix}/TreeViewer/io.github.arklumpus.TreeViewer.desktop"

USER_HOME=$(getent passwd ${SUDO_USER:-$USER} | cut -d: -f6)
rm -rf "${USER_HOME}"/.local/share/TreeViewer/modules*

printf "\nWe will now add TreeViewer to the desktop menu. You\nshould only skip this step if you are installing TreeViewer\non a system without a desktop enviroment.\n"

confirm="a"

while [ "$confirm" != "Y" ] && [ "$confirm" != "y" ] && [ "$confirm" != "N" ] && [ "$confirm" != "n" ]; do
    read -p "Do you wish to continue? [Y/n] " confirm
done

if [ "$confirm" != "Y" ] && [ "$confirm" != "y" ]; then
    printf "\nTreeViewer was not added to the desktop menu.\n"
else
    xdg-icon-resource install --novendor --context apps --size 16 "${prefix}/TreeViewer/Icons/Program-16.png" io.github.arklumpus.TreeViewer
    xdg-icon-resource install --novendor --context apps --size 32 "${prefix}/TreeViewer/Icons/Program-32.png" io.github.arklumpus.TreeViewer
    xdg-icon-resource install --novendor --context apps --size 48 "${prefix}/TreeViewer/Icons/Program-48.png" io.github.arklumpus.TreeViewer
    xdg-icon-resource install --novendor --context apps --size 64 "${prefix}/TreeViewer/Icons/Program-64.png" io.github.arklumpus.TreeViewer
    xdg-icon-resource install --novendor --context apps --size 256 "${prefix}/TreeViewer/Icons/Program-256.png" io.github.arklumpus.TreeViewer
    xdg-icon-resource install --novendor --context apps --size 512 "${prefix}/TreeViewer/Icons/Program-512.png" io.github.arklumpus.TreeViewer
    xdg-desktop-menu install --novendor "${prefix}/TreeViewer/io.github.arklumpus.TreeViewer.desktop"

    printf "\nTreeViewer was added to the desktop menu.\n"
fi

printf "\nWe will now create symlinks to the executables TreeViewer\nand TreeViewerCommandLine in /usr/bin\n"

confirm="a"

while [ "$confirm" != "Y" ] && [ "$confirm" != "y" ] && [ "$confirm" != "N" ] && [ "$confirm" != "n" ]; do
    read -p "Do you wish to continue? [Y/n] " confirm
done

if [ "$confirm" != "Y" ] && [ "$confirm" != "y" ]; then
    printf "\nSymlinks were not created.\n"
    exit 0
else
    ln -sf "${prefix}/TreeViewer/TreeViewer" "${prefix}/TreeViewer/TreeViewerCommandLine" /usr/bin
    printf "\nSymlinks created.\n"
fi

printf "\nInstallation complete!\n\n"
