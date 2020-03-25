################################################################
# Basic paths

BASE_PATH=$(dirname $0)/../
ASSET_PATH=$BASE_PATH/../../
PROJECT_PATH=$ASSET_PATH/../

BASE_PATH=$(cd $BASE_PATH && pwd)
ASSET_PATH=$(cd $ASSET_PATH && pwd)
PROJECT_PATH=$(cd $PROJECT_PATH && pwd)

echo "BASE_PATH:    " $BASE_PATH
echo "ASSET_PATH:   " $ASSET_PATH
echo "PROJECT_PATH: " $PROJECT_PATH

echo ""

################################################################
# Unity environments

PROJECT_VERSION_PATH=$PROJECT_PATH/ProjectSettings/ProjectVersion.txt
if [[ ! -f $PROJECT_VERSION_PATH ]]; then
  echo "ERROR: Unity version file not found ($PROJECT_VERSION_PATH)"
  exit 2
fi

UNITY_VER=`cat $PROJECT_VERSION_PATH | sed 's/.*: //' | tr -d '\n'`
UNITY_APP=/Applications/Unity/Hub/Editor/$UNITY_VER/Unity.app/Contents/MacOS/Unity

echo "UNITY_VER:    " $UNITY_VER
echo "UNITY_APP:    " $UNITY_APP

echo -n "Check Unity.. "
if [ ! -f $UNITY_APP ]; then
  echo "NG"
  echo "ERROR: Unity Application not found"
  exit 1
fi
echo "OK"

echo ""
