SCRIPT_DIR=$(cd $(dirname $0); pwd)/../../../../

pushd $SCRIPT_DIR
/Applications/Unity/Hub/Editor/2018.3.13f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -logFile ./build.log -projectPath ./ -executeMethod Mmzk.BuildScript.Build -outputname app-develop.apk -buildsettings target/android:flavor/app:environment/develop
cat build.log
popd
