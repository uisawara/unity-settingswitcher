# build.sh <unity-version> <settings-envlist ex: flavors/SceneA+targets/Android+buildtype/develop> <args..>
cd $(dirname $0)/../../../../
unity_version=$1
envlist=$2
shift 2
/Applications/Unity/Hub/Editor/$unity_version/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -logFile /dev/stdout -projectPath . -executeMethod uisawara.BuildScript.Build --envlist $envlist $@
