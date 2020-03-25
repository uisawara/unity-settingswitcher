# build.sh <settings-envlist ex: flavors/SceneA+targets/Android+buildtype/develop> <args..>
source ./envs.sh || exit $?
envlist=$1
shift 1
$UNITY_APP -batchmode -nographics -quit -logFile /dev/stdout -projectPath $PROJECT_PATH -executeMethod uisawara.BuildScript.Build --envlist $envlist $@
