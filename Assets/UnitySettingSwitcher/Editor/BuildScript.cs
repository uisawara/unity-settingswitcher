using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace uisawara
{
    public class BuildScript
    {
        public static void ApplyBuildCmdargs()
        {
            // Parse command line args & create settings
            var cmdsettings = new CmdSettings();
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
                if (args[i] == "--envlist")
                {
                    var envlist = args[++i];
                    cmdsettings.EnvList = envlist.Split('+');
                }
                else if (args[i] == "--version")
                {
                    var versionName = args[++i];

                    //
                    PlayerSettings.bundleVersion = versionName;
                    Debug.Log(" - bundleVersion=" + versionName);

                    //
                    var vns = versionName.Split('.');
                    var versionCode = 0;
                    var mul = 1000000;
                    foreach (var vn in vns)
                    {
                        versionCode += int.Parse(vn) * mul;
                        mul /= 100;
                    }

                    PlayerSettings.Android.bundleVersionCode = versionCode;
                    Debug.Log(" - Android.bundleVersionCode =" + versionCode);
                }

            Debug.Log("EnvList: " + string.Join("+", cmdsettings.EnvList));

            // Switch settings
            var settings = SettingsUtil.LoadBuildSettings();
            var env = SettingsUtil.Create(settings, cmdsettings.EnvList);
            SettingsUtil.ChangeBuildSettings(env);
        }

        public static void Build()
        {
            ApplyBuildCmdargs();
            BuildForActiveBuildTarget();
        }

        [MenuItem("Tools/Build active target")]
        public static void BuildForActiveBuildTarget()
        {
            // Start build
            var outputPath = Path.Combine(Application.dataPath, "../Build",
                EditorUserBuildSettings.activeBuildTarget.ToString());
            var outputName = PlayerSettings.productName;
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) outputName = outputName + ".apk";
            Directory.CreateDirectory(outputPath);
            BuildPipeline.BuildPlayer(
                GetAllScenePaths(),
                Path.Combine(outputPath, outputName),
                EditorUserBuildSettings.activeBuildTarget,
                BuildOptions.None);
        }

        private static string[] GetAllScenePaths()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++) scenes[i] = EditorBuildSettings.scenes[i].path;
            return scenes;
        }

        public static void ExportUnitypackage()
        {
            // Parse command line args & create settings
            string assetPathNames = null;
            string fileName = null;
            ExportPackageOptions exportPackageOptions = 0;
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
                if (args[i] == "--assetPathNames")
                {
                    assetPathNames = args[++i];
                }
                else if (args[i] == "--fileName")
                {
                    fileName = args[++i];
                }
                else if (args[i] == "--ExportPackageOptions")
                {
                    var epo = args[++i];
                    var epos = epo.Split('+');
                    foreach (var e in epos)
                        exportPackageOptions |= (ExportPackageOptions) Enum.Parse(typeof(ExportPackageOptions), e);
                }

            Debug.Log($"ExportUnitypackage: {assetPathNames}, {fileName}, {(uint) exportPackageOptions}");

            //
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            AssetDatabase.ExportPackage(assetPathNames, fileName, exportPackageOptions);
        }

        public class CmdSettings
        {
            public string[] EnvList { get; set; } = new string[0];
        }
    }
}