using System;
using System.Collections.Generic;
using System.IO;
using uisawara;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mmzk
{

    /// <summary>
    /// Build script.
    /// コマンドラインビルド向けのスクリプト.
    /// </summary>
    public class BuildScript
    {

        public static void Build()
        {

            var args = Environment.GetCommandLineArgs();

            // Parse build parameters, And create build settings.
            Settings settings = SettingsUtil.LoadBuildSettings();

            Settings.Environment env = null;
            string buildTargetGroup = "unknown";
            string buildTarget = "notarget";
            var sceneList = new List<string>();

            Console.WriteLine("----------------------------------------------------------------");
            string buildOutputPath = Path.Combine(Application.dataPath, "../Build");
            string buildOutputName = "output";
            var buildOptions = BuildOptions.None;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildsettings")
                {
                    string envargs = args[++i];
                    var names = envargs.Split(':');
                    env = SettingsUtil.Create(settings, names);

                    if (env.build_settings.ContainsKey("build_target_group") != null)
                    {
                        buildTargetGroup = (string)env.build_settings["build_target_group"];
                    }
                    if (env.build_settings.ContainsKey("build_target") != null)
                    {
                        buildTarget = (string)env.build_settings["build_target"];
                    }
                    if (env.scene_list != null)
                    {
                        sceneList = env.scene_list;
                    }
                }
                else if (args[i] == "-outputname")
                {
                    buildOutputName = args[++i];
                }
            }

            // Execute build
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("[ExecuteBuild]");
            Console.WriteLine("OutputName : " + buildOutputName);
            Console.WriteLine("buildTargetGroup : " + buildTargetGroup);
            Console.WriteLine("buildTarget : " + buildTarget);
            Console.WriteLine("sceneList : " + string.Join(":", sceneList));
            Console.WriteLine("----------------------------------------------------------------");
            string outputFullPath = Path.Combine(buildOutputPath, buildOutputName);
            SettingsUtil.ChangeBuildSettings(env);
            var buildReport = BuildPipeline.BuildPlayer(
                sceneList.ToArray(),
                outputFullPath,
                (BuildTarget)Enum.Parse(typeof(BuildTarget), buildTarget, true),
                buildOptions
            );
            Console.WriteLine("----------------------------------------------------------------");
            var buildSummary = buildReport.summary;
            if (buildSummary.result == BuildResult.Succeeded)
            {
                Console.WriteLine("[BuildResult] Build succeeded: " + buildSummary.totalSize + " bytes");
            }

            if (buildSummary.result == BuildResult.Failed)
            {
                Console.WriteLine("[BuildResult] Build failed");
            }
            Console.WriteLine(" - totalErrors: " + buildSummary.totalErrors);
            Console.WriteLine(" - totalWarnings: " + buildSummary.totalWarnings);
            Console.WriteLine("----------------------------------------------------------------");

        }

    }

}
