using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR;

namespace uisawara
{

    public static class SettingsUtil
    {

        public static void CreateBuildsettingsFromTemplate(string jsonPath)
        {
            string jsonTemplatePath = SettingConstants.SETTING_TEMPLATE_FILE_PATHNAME;
            File.Copy(jsonTemplatePath, jsonPath);
            AssetDatabase.Refresh();
        }

        public static Settings LoadBuildSettings()
        {
            string jsonPath = Path.Combine(Application.dataPath, SettingConstants.SETTING_FILE_NAME);
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                var data = JsonUtility.FromJson<Settings>(json);
                return data;
            }
            return null;
        }

        public static Settings.Environment Create(Settings buildSettings, string[] envList)
        {
            var sb = new StringBuilder();

            // 環境設定の連結・継承解決
            var resolvedEnvList = new List<string>();
            foreach(var env in envList)
            {
                var inherit = buildSettings.ResolveInherit(env);
                resolvedEnvList.AddRange(inherit);

                sb.Append(string.Join(" < ", inherit));
                sb.Append("] x [");
            }
            if(sb.Length>=1)
            {
                sb.Remove(sb.Length - 5, 5);
            }
            Debug.Log(" - EnvironmentTree: [" + sb.ToString() + "]");

            // 環境設定の生成
            var result = new Settings.Environment();
            foreach(var env in resolvedEnvList)
            {
                var index = buildSettings.GetEnvironmentIndex(env);
                if (index == -1)
                {
                    throw new ArgumentNullException("setting not found: " + env);
                }
                var current = buildSettings.build_settings[index];
                result = Settings.Merge(result, current);
            }
            return result;
        }

        public static Settings.Environment Copy(Settings.Environment lhs)
        {
            string tmp = JsonUtility.ToJson(lhs);
            return JsonUtility.FromJson<Settings.Environment>(tmp);
        }

        public static void ChangeBuildSettings(Settings.Environment buildSettings)
        {

            var settingLog = new StringBuilder();
            settingLog.Append(" - Change BuildEnvironment: ");
            settingLog.AppendLine(buildSettings.name);
            settingLog.AppendLine(JsonUtility.ToJson(buildSettings));
            Debug.Log(settingLog.ToString());

            // TargetPlatform
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildSettings.build_settings != null)
            {
                if (buildSettings.build_settings.dictionary.ContainsKey("build_target"))
                {
                    buildTargetGroup = (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), buildSettings.build_settings.dictionary["build_target_group"].s, true);
                }
                if (buildSettings.build_settings.dictionary.ContainsKey("build_target"))
                {
                    buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), buildSettings.build_settings.dictionary["build_target"].s, true);
                }
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
            }
            Debug.Log(" - BuildTargetGroup: " + buildTargetGroup.ToString());
            Debug.Log(" - BuildTarget: " + buildTarget.ToString());

            // *Settings
            if (buildSettings.player_settings != null)
            {
                SettingsUtil.ApplySettings(typeof(PlayerSettings), buildSettings.player_settings);
            }
            if (buildSettings.xr_settings != null)
            {
                SettingsUtil.ApplySettings(typeof(XRSettings), buildSettings.xr_settings);
            }

            // ScriptingDefineSymbols
            if (buildSettings.scripting_define_symbols != null)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, buildSettings.scripting_define_symbols);
                Debug.Log(" - ScriptingDefineSymbols: " + buildSettings.scripting_define_symbols);
            }

            // Scenes
            if (buildSettings.scene_list != null)
            {
                // ターゲットプラットフォーム
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

                // シーン構成
                var scenes = new EditorBuildSettingsScene[buildSettings.scene_list.Count];
                for (int i = 0; i < scenes.Length; i++)
                {
                    scenes[i] = new EditorBuildSettingsScene(buildSettings.scene_list[i], true);
                };
                EditorBuildSettings.scenes = scenes;

                // エディタ・シーンリスト
                for (int i = 0; i < scenes.Length; i++)
                {
                    EditorSceneManager.OpenScene(buildSettings.scene_list[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
                };
            }

        }

        private static void ApplySettings(Type target, KeyValueDictionary data)
        {
            var fis = target.GetMembers(BindingFlags.Public | BindingFlags.Static);
            var sb = new StringBuilder();
            sb.AppendLine(" - " + target.Name + ":");
            foreach (var kv in data.dictionary)
            {
                var p = target.GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Static);
                var pt = p.PropertyType;
                if (p == null)
                {
                    sb.AppendLine(" - unknown key: " + kv.Key);
                    continue;
                }

                if (pt == typeof(String))
                {
                    p.SetValue(null, kv.Value.s);
                    sb.Append("    - ");
                    sb.AppendLine(kv.Key + " = " + kv.Value.s);
                }
                else
                if (pt == typeof(Boolean))
                {
                    p.SetValue(null, kv.Value.b);
                    sb.Append("    - ");
                    sb.AppendLine(kv.Key + " = " + kv.Value.b);
                }
                else
                if (pt == typeof(int))
                {
                    p.SetValue(null, kv.Value.i);
                    sb.Append("    - ");
                    sb.AppendLine(kv.Key + " = " + kv.Value.i);
                }
                else
                if (pt == typeof(float))
                {
                    p.SetValue(null, kv.Value.f);
                    sb.Append("    - ");
                    sb.AppendLine(kv.Key + " = " + kv.Value.f);
                }
            }
            Debug.Log(sb.ToString());
        }

    }

}
