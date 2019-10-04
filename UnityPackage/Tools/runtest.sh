SCRIPT_DIR=$(cd $(dirname $0); pwd)/../../../../

pushd $SCRIPT_DIR
/Applications/Unity/Hub/Editor/2018.3.13f1/Unity.app/Contents/MacOS/Unity -runTests -testResults ./testResults-editmode.xml -projectPath . -testPlatform editmode
/Applications/Unity/Hub/Editor/2018.3.13f1/Unity.app/Contents/MacOS/Unity -runTests -testResults ./testResults-playmode.xml -projectPath . -testPlatform playmode
popd
