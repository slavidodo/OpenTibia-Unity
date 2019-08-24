#! /bin/sh

# Download Unity3D installer into the container
echo 'Downloading Unity 2019.1.8f pkg:'
curl --retry 5 -o Unity.pkg https://download.unity3d.com/download_unity/7938dd008a75/MacEditorInstaller/Unity.pkg
if [ $? -ne 0 ]; then { echo "Download failed"; exit $?; } fi

# TODO download modules to support other platforms (windows, linux, ...)

echo 'Installing Unity_MacOS.pkg'
sudo installer -dumplog -package Unity.pkg -target /

echo "Verify firewall"
/usr/libexec/ApplicationFirewall/socketfilterfw --getappblocked /Applications/Unity/Unity.app/Contents/MacOS/Unity

echo "Create Certificate Folder"
mkdir ~/Library/Unity
mkdir ~/Library/Unity/Certificates

cp CACerts.pem ~/Library/Unity/Certificates/