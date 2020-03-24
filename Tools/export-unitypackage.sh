# Assets/UnitySettingSwitcher/Tools/export-unitypackage.sh 2018.4.14f1 Assets/UnitySettingSwitcher Build/unitypackages/UnitySettingSwitcher.unitypackage Recurse
cd $(dirname $0)/../../../../
/Applications/Unity/Hub/Editor/$1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -logFile /dev/stdout -projectPath . -executeMethod uisawara.BuildScript.ExportUnitypackage --assetPathNames $2 --fileName $3 --ExportPackageOptions $4
