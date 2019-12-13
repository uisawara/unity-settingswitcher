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
using static UnityEditor.PlayerSettings;

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
            var result = new Settings();
            LoadBuildSettings(Path.Combine(Application.dataPath, SettingConstants.SETTING_FILE_NAME), result);
            var settingFiles = Directory.GetFiles(Application.dataPath, SettingConstants.SETTING_LOCAL_FILE_PATHNAME);
            foreach(var sf in settingFiles)
            {
                LoadBuildSettings(Path.Combine(Application.dataPath, sf), result);
            }
            return result;
        }

        private static void LoadBuildSettings(string settingFilePath, Settings result)
        {
            if (File.Exists(settingFilePath))
            {
                string json = File.ReadAllText(settingFilePath);
                var data_ = Json.Deserialize(json) as Dictionary<string, object>;

                var settings = (IList)data_["settings"];
                foreach (var s in settings)
                {
                    var se = new Settings.Environment();
                    result.settings.Add(se);

                    // TODO If you add setting item then add process to here
                    var bs = (IDictionary)s;
                    se.name = bs.Contains("name") ? (string)bs["name"] : null;
                    se.inherit = bs.Contains("inherit") ? (string)bs["inherit"] : null;
                    se.scene_list = bs.Contains("scene_list") ? ((List<object>)bs["scene_list"]).Select(x => (string)x).ToList<string>() : null;
                    se.build_settings = bs.Contains("build_settings") ? (Dictionary<string, object>)bs["build_settings"] : null;
                    se.player_settings = bs.Contains("player_settings") ? (Dictionary<string, object>)bs["player_settings"] : null;
                    se.editor_user_build_settings = bs.Contains("editor_user_build_settings") ? (Dictionary<string, object>)bs["editor_user_build_settings"] : null;
                    se.android = bs.Contains("android") ? (Dictionary<string, object>)bs["android"] : null;
                    se.xr_settings = bs.Contains("xr_settings") ? (Dictionary<string, object>)bs["xr_settings"] : null;
                }
            }
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

        /// <summary>
        /// Changes the build settings.
        /// Apply settings to Unity Editor static properties from settingenvironment values.
        /// </summary>
        /// <param name="settingenvironment">Settingenvironment.</param>
        public static void ChangeBuildSettings(Settings.Environment settingenvironment)
        {

            var settingLog = new StringBuilder();
            settingLog.Append(" - Change BuildEnvironment: ");
            settingLog.AppendLine(settingenvironment.name);
            settingLog.AppendLine(JsonUtility.ToJson(settingenvironment));
            Debug.Log(settingLog.ToString());

            // TargetPlatform
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (settingenvironment.build_settings != null)
            {
                if (settingenvironment.build_settings.ContainsKey("build_target_group"))
                {
                    buildTargetGroup = (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), (string)settingenvironment.build_settings["build_target_group"], true);
                }

                if (settingenvironment.build_settings.ContainsKey("build_target"))
                {
                    buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), (string)settingenvironment.build_settings["build_target"], true);

                    // ターゲットプラットフォーム
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
                }

            }
            Debug.Log(" - BuildTargetGroup: " + buildTargetGroup.ToString());
            Debug.Log(" - BuildTarget: " + buildTarget.ToString());

            // *Settings
            // TODO If you add setting item then add applysettings process to here.

            if (settingenvironment.player_settings != null)
            {
                SettingsUtil.ApplySettings(typeof(PlayerSettings), settingenvironment.player_settings);

                if (settingenvironment.player_settings.ContainsKey("ScriptingBackend"))
                {
                    var value = (ScriptingImplementation)Enum.Parse(typeof(ScriptingImplementation), (string)settingenvironment.player_settings["ScriptingBackend"], true);
                    PlayerSettings.SetPropertyInt("ScriptingBackend", (int)value, buildTargetGroup);
                }

            }
            if (settingenvironment.editor_user_build_settings != null)
            {
                SettingsUtil.ApplySettings(typeof(EditorUserBuildSettings), settingenvironment.editor_user_build_settings);
            }
            if (settingenvironment.xr_settings != null)
            {
                SettingsUtil.ApplySettings(typeof(XRSettings), settingenvironment.xr_settings);
            }
            if (settingenvironment.android != null)
            {
                SettingsUtil.ApplySettings(typeof(Android), settingenvironment.android);

                if (settingenvironment.android.ContainsKey("targetArchitectures"))
                {
                    var value = settingenvironment.android["targetArchitectures"] as string;
                    var tokens = value.Split(new char[] { ' ' });
                    uint flag = 0;
                    foreach (var t in tokens)
                    {
                        flag |= (uint)Enum.Parse(typeof(AndroidArchitecture), (string)t, true);
                    }
                    Android.targetArchitectures = (AndroidArchitecture)flag;
                }
            }

            // ScriptingDefineSymbols
            if (settingenvironment.player_settings != null && settingenvironment.player_settings.ContainsKey("scripting_define_symbols"))
            {
                var v = (string)settingenvironment.player_settings["scripting_define_symbols"];
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, v);
                Debug.Log(" - ScriptingDefineSymbols: " + v);
            }

            // Scenes
            if (settingenvironment.scene_list != null)
            {

                // シーン構成
                var scenes = new EditorBuildSettingsScene[settingenvironment.scene_list.Count];
                for (int i = 0; i < scenes.Length; i++)
                {
                    scenes[i] = new EditorBuildSettingsScene(settingenvironment.scene_list[i], true);
                };
                EditorBuildSettings.scenes = scenes;

                // エディタ・シーンリスト
                for (int i = 0; i < scenes.Length; i++)
                {
                    EditorSceneManager.OpenScene(settingenvironment.scene_list[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
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
                if (p == null)
                {
                    Debug.Log(" - unknown key: " + key);
                    continue;
                }

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
