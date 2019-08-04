using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MiniJSON;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;

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
                var result = new Settings();

                string json = File.ReadAllText(jsonPath);
                var data_ = Json.Deserialize(json) as Dictionary<string, object>;

                var settings = (IList)data_["settings"];
                foreach(var s in settings)
                {
                    var se = new Settings.Environment();
                    result.settings.Add(se);

                    var bs = (IDictionary)s;
                    se.name = bs.Contains("name")? (string)bs["name"] : null;
                    se.inherit = bs.Contains("inherit")? (string)bs["inherit"] : null;
                    se.scene_list = bs.Contains("scene_list") ? ((List<object>)bs["scene_list"]).Select(x=>(string)x).ToList<string>() : null;
                    se.build_settings = bs.Contains("build_settings")? (Dictionary<string, object>)bs["build_settings"] : null;
                    se.player_settings = bs.Contains("player_settings")? (Dictionary<string, object>)bs["player_settings"] : null;
                    se.xr_settings = bs.Contains("xr_settings")? (Dictionary<string, object>)bs["xr_settings"] : null;
                }

                return result;
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
            Debug.Log(" - Settings: [" + sb.ToString() + "]");

            // 環境設定の生成
            var result = new Settings.Environment();
            foreach(var env in resolvedEnvList)
            {
                var index = buildSettings.GetEnvironmentIndex(env);
                if (index == -1)
                {
                    throw new ArgumentNullException("setting not found: " + env);
                }
                var current = buildSettings.settings[index];
                Settings.Merge(result, current);
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
                if (buildSettings.build_settings.ContainsKey("build_target"))
                {
                    buildTargetGroup = (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), (string)buildSettings.build_settings["build_target_group"], true);
                }
                if (buildSettings.build_settings.ContainsKey("build_target"))
                {
                    buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), (string)buildSettings.build_settings["build_target"], true);
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
            if (buildSettings.player_settings.ContainsKey("scripting_define_symbols"))
            {
                var v = (string)buildSettings.player_settings["scripting_define_symbols"];
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, v);
                Debug.Log(" - ScriptingDefineSymbols: " + v);
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

        private static void ApplySettings(Type target, Dictionary<string, object> data)
        {
            var fis = target.GetMembers(BindingFlags.Public | BindingFlags.Static);
            var sb = new StringBuilder();
            sb.AppendLine(" - " + target.Name + ":");
            foreach (var kv in data)
            {
                var key = kv.Key;
                var value = kv.Value;

                // Skip special keys.
                if(key== "scripting_define_symbols")
                {
                    continue;
                }

                var p = target.GetProperty(key, BindingFlags.Public | BindingFlags.Static);
                var pt = p.PropertyType;
                if (p == null)
                {
                    sb.AppendLine(" - unknown key: " + key);
                    continue;
                }

                sb.Append("    - ");
                sb.AppendLine(key + " = " + value);
                Debug.Log(sb.ToString());

                if (pt == typeof(String))
                {
                    var v = (String)value;
                    p.SetValue(null, v);
                }
                else
                if (pt == typeof(Boolean))
                {
                    p.SetValue(null, (Boolean)value);
                }
                else
                if (pt == typeof(int))
                {
                    p.SetValue(null, (int)value);
                }
                else
                if (pt == typeof(float))
                {
                    p.SetValue(null, (float)value);
                }
            }

        }

    }

}
