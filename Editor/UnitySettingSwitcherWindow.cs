using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace uisawara
{
    /// Overview
    /// - ビルド設定定義ファイルにより管理しやすいビルド設定管理
    /// - ビルド設定の組み合わせにより構成管理を容易にする
    /// - ビルド設定の継承により設定差分の管理を容易にする
    /// - 設定のグループ分け
    /// - 設定の継承
    /// <summary>
    ///     Build environment window.
    ///     ビルド環境を切り替えるUnityEditor拡張です。
    /// </summary>
    public class UnitySettingSwitcherWindow : EditorWindow
    {
        private static Settings settings;
        private static readonly SettingsSelector settingsSelector = new SettingsSelector();

        [MenuItem("Window/Unity Setting Switcher %e")]
        private static void Open()
        {
            GetWindow<UnitySettingSwitcherWindow>(false, "Unity Setting Switcher");
        }

        private void OnGUI()
        {
            var jsonPath = Path.Combine(Application.dataPath, SettingConstants.SETTING_FILE_NAME);
            settingsSelector.LoadBuildSettingsSelected();

            if (settings == null)
            {
                Reload();
                if (settings == null)
                {
                    GUILayout.Label("Setup Setting.json first.");
                    return;
                }
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Utility:");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("BuildSettings"))
            {
                GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true, "Build Environment");
            }

            //if (GUILayout.Button("PlayerSettings"))
            //{
            //    // Inspector
            //    Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            //}
            if (GUILayout.Button("PlayerSettings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
                ;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dump CurrentSceneList"))
            {
                var sceneList = string.Join("\n", EditorBuildSettings.scenes.Select(x => "\"" + x.path + "\","));
                if (sceneList.Length == 0)
                {
                    return;
                }

                sceneList += Environment.NewLine;

                Debug.Log(sceneList);
            }

            if (GUILayout.Button("Open Buildscript"))
            {
                Process.Start(jsonPath);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Build environments
            GUILayout.BeginHorizontal();
            GUILayout.Label("Build Environments:");
            if (GUILayout.Button("Reload"))
            {
                Reload();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Setting buttons
            var group = "";
            foreach (var bs in settings.settings)
            {
                var name = bs.name;

                var index = bs.name.LastIndexOf('/');
                if (index != -1)
                {
                    var g = bs.name.Substring(0, index);
                    name = bs.name.Substring(index + 1);
                    if (group != g)
                    {
                        group = g;
                        GUILayout.Label(group);
                    }
                }

                if (name[0] != '.')
                {
                    var activeEnv = settingsSelector.IsSelected(bs.name);
                    GUI.backgroundColor = activeEnv ? Color.gray : new Color(32, 96, 128);

                    if (GUILayout.Button((activeEnv ? "*" : "") + name))
                    {
                        settingsSelector.LoadBuildSettingsSelected();
                        settingsSelector.Select(bs.name);
                        settingsSelector.SaveBuildSettingsSelected();

                        var envlist = settingsSelector.buildSettingsSelected.environmentPaths.ToArray();
                        Debug.Log("ApplyEnv: " + string.Join("+", envlist));
                        var env = SettingsUtil.Create(settings, envlist);
                        SettingsUtil.ChangeBuildSettings(env);
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void Reload()
        {
            var buildSettings = SettingsUtil.LoadBuildSettings();
            settings = buildSettings;
            Debug.Log("reload " + SettingConstants.SETTING_FILE_NAME + " finished");
        }
    }
}