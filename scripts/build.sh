#! /bin/sh

# Activating temporary licence
echo "Activating Unity licence"
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -serial I3-GKE5-PKF4-XXXX-XXXX-XXXX -username $username -password 'xxxxxxxx' -logfile

cat ~/Library/Logs/Unity/Editor.log

# Processing builds
echo "Building ${UNITYCI_PROJECT_NAME} for OSX"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile $(pwd)/unity.log \
	-projectPath "$(pwd)/${UNITYCI_PROJECT_NAME}" \
	-buildOSXUniversalPlayer "$(pwd)/Build/osx/${UNITYCI_PROJECT_NAME}.app" \
	-quit

rc0=$?
echo "Build logs (OSX)"
cat $(pwd)/unity.log

echo "Returning Unity licence"
/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -returnlicense -logfile

if [ $rc0 -ne 0 ]; then { echo "Failed OSX Build"; } fi

exit $rc0