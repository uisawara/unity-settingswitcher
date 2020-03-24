# test.sh <unity-version> <settings-envlist ex: flavors/SceneA+targets/Android+buildtype/develop>
cd $(dirname $0)/../../../../
/Applications/Unity/Hub/Editor/$1/Unity.app/Contents/MacOS/Unity -batchmode -logFile /dev/stdout -projectPath . -executeMethod uisawara.BuildScript.ApplyBuildCmdargs --envlist $2 -runTests -testPlatform playmode -editorTestsResultFile ./Tests/playmode-result.xml
/Applications/Unity/Hub/Editor/$1/Unity.app/Contents/MacOS/Unity -batchmode -logFile /dev/stdout -projectPath . -executeMethod uisawara.BuildScript.ApplyBuildCmdargs --envlist $2 -runEditorTests -editorTestsResultFile ./Tests/editormode-result.xml
