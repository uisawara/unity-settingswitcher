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
using UnityEditor.Build.Content;

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
            foreach (var sf in settingFiles)
            {
                LoadBuildSettings(sf, result);
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
                    se.build_settings = bs.Contains("build_settings") ? (Dictionary<string, object>)bs["build_settings"] : null;
                    se.player_settings = bs.Contains("player_settings") ? (Dictionary<string, object>)bs["player_settings"] : null;
                    se.editor_build_settings = bs.Contains("editor_build_settings") ? (Dictionary<string, object>)bs["editor_build_settings"] : null;
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
            foreach (var env in envList)
            {
                var inherit = buildSettings.ResolveInherit(env);
                resolvedEnvList.AddRange(inherit);

                sb.Append(string.Join(" < ", inherit));
                sb.Append("] x [");
            }
            if (sb.Length >= 1)
            {
                sb.Remove(sb.Length - 5, 5);
            }
            Debug.Log(" - Settings: [" + sb.ToString() + "]");

            // 環境設定の生成
            var result = new Settings.Environment();
            foreach (var env in resolvedEnvList)
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
                SettingsUtil.ApplySettings(typeof(BuildSettings), settingenvironment.build_settings, new Dictionary<string, Action<object, object>>()
                {
                    {"build_target_group", (target, value) => {
                        buildTargetGroup = (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), (string)settingenvironment.build_settings["build_target_group"], true);
                    }},
                    {"build_target", (target, value) => {
                        buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), (string)settingenvironment.build_settings["build_target"], true);
                    }},
                });

                // ターゲットプラットフォーム切替
                if (settingenvironment.build_settings.ContainsKey("build_target"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
                }

            }
            Debug.Log(" - BuildTargetGroup: " + buildTargetGroup.ToString());
            Debug.Log(" - BuildTarget: " + buildTarget.ToString());

            // *Settings
            // TODO If you add setting item then add applysettings process to here.

            if (settingenvironment.player_settings != null)
            {
                SettingsUtil.ApplySettings(typeof(PlayerSettings), settingenvironment.player_settings, new Dictionary<string, Action<object, object>>()
                {
                    {"scripting_define_symbols", (target, value) => {
                        Debug.Log(" - ScriptingDefineSymbols: " + (string)value);
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, (string)value);
                    }},
                    {"ScriptingBackend", (target, value) => {
                        Debug.Log(" - scriptingBackend: " + (string)value);
                        PlayerSettings.SetScriptingBackend(buildTargetGroup, (ScriptingImplementation)Enum.Parse(typeof(ScriptingImplementation), (string)value));
                    }},
                    {"VirtualRealitySDKs", (target, value) => {
                        Debug.Log(" - VirtualRealitySDKs: " + (string)value);
                        PlayerSettings.SetVirtualRealitySDKs(EditorUserBuildSettings.selectedBuildTargetGroup, ((string)value).Split(new char[]{' ' }));
                    }},
                    {"applicationIdentifier", (target, value) => {
                        Debug.Log(" - applicationIdentifier: " + (string)value);
                        //PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, (string)value);
                        PlayerSettings.SetApplicationIdentifier(buildTargetGroup, (string)value);
                    }},
                });
            }
            //if (settingenvironment.editor_build_settings != null)
            //{
            //    SettingsUtil.ApplySettings(typeof(EditorBuildSettings), settingenvironment.editor_build_settings, new Dictionary<string, Action<object, object>>()
            //    {
            //        {"scenes", (target, value) => {
            //        }},
            //    });
            //}
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

        /// <summary>
        /// Apply settings to The target class.
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="data">Data.</param>
        private static void ApplySettings(Type target, Dictionary<string, object> data, Dictionary<string, Action<object, object>> customDispatcher = null)
        {
            var fis = target.GetMembers(BindingFlags.Public | BindingFlags.Static);
            var sb = new StringBuilder();
            sb.AppendLine(" - " + target.Name + ":");
            foreach (var kv in data)
            {
                var key = kv.Key;
                var value = kv.Value;

                if (customDispatcher != null && customDispatcher.ContainsKey(key))
                {
                    customDispatcher[key](target, value);
                }
                else
                {

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
                    else
                    if (pt.IsEnum)
                    {
                        var fa = pt.GetCustomAttribute(typeof(FlagsAttribute));
                        if (fa == null)
                        {
                            p.SetValue(null, Enum.Parse(pt, (string)value));
                        }
                        else
                        {
                            var tokens = ((string)value).Split(new char[] { ' ' });
                            uint flag = 0;
                            foreach (var t in tokens)
                            {
                                flag |= (uint)Enum.Parse(pt, (string)t, true);
                            }
                            p.SetValue(null, flag);
                        }
                    }
                    else
                    {
                        Debug.LogError(" - unknown type: " + key);
                    }

                }

            }

        }

    }

}
