# test.sh <settings-envlist ex: flavors/SceneA+targets/Android+buildtype/develop>
source $(dirname $0)/envs.sh || exit $?
$UNITY_APP -batchmode -logFile /dev/stdout -projectPath $PROJECT_PATH -executeMethod uisawara.BuildScript.ApplyBuildCmdargs --envlist $1 -runTests -testPlatform playmode -editorTestsResultFile $PROJECT_PATH/Tests/playmode-result.xml
$UNITY_APP -batchmode -logFile /dev/stdout -projectPath $PROJECT_PATH -executeMethod uisawara.BuildScript.ApplyBuildCmdargs --envlist $1 -runEditorTests -editorTestsResultFile $PROJECT_PATH/Tests/editormode-result.xml
