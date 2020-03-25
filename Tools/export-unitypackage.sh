# export-unitypackage.sh Assets/UnitySettingSwitcher Build/unitypackages/UnitySettingSwitcher.unitypackage Recurse
source ./envs.sh || exit $?
$UNITY_APP -batchmode -nographics -quit -logFile /dev/stdout -projectPath $PROJECT_PATH -executeMethod uisawara.BuildScript.ExportUnitypackage --assetPathNames $1 --fileName $2 --ExportPackageOptions $3
