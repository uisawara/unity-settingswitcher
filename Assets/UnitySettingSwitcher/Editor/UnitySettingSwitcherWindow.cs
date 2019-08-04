using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace uisawara
{

    /// Overview
    /// - ビルド設定定義ファイルにより管理しやすいビルド設定管理
    /// - ビルド設定の組み合わせにより構成管理を容易にする
    /// - ビルド設定の継承により設定差分の管理を容易にする
    ///     - 設定のグループ分け
    ///     - 設定の継承

    /// <summary>
    /// Build environment window.
    /// ビルド環境を切り替えるUnityEditor拡張です。
    /// </summary>
    public class UnitySettingSwitcherWindow : EditorWindow
    {

        private static Settings buildSettings;
        private static SettingsSelector buildSettingsSelector = new SettingsSelector();

        [MenuItem("Window/Unity Setting Switcher %e")]
        static void Open()
        {
            GetWindow<UnitySettingSwitcherWindow>(false, "Unity Setting Switcher");
        }

        void OnGUI()
        {

            string jsonPath = Path.Combine(Application.dataPath, SettingConstants.SETTING_FILE_NAME);
            buildSettingsSelector.LoadBuildSettingsSelected();

            if (UnitySettingSwitcherWindow.buildSettings == null)
            {
                Reload();
                if (buildSettings == null)
                {
                    if (GUILayout.Button("Create " + SettingConstants.SETTING_FILE_NAME + " from template"))
                    {
                        SettingsUtil.CreateBuildsettingsFromTemplate(jsonPath);
                    }
                    return;
                }
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Utility:");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("BuildSettings"))
            {
                EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true, "Build Environment");
            }
            //if (GUILayout.Button("PlayerSettings"))
            //{
            //    // Inspector
            //    Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            //}
            if (GUILayout.Button("PlayerSettings"))
            {
                SettingsService.OpenProjectSettings("Project/Player"); ;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dump CurrentSceneList"))
            {
                var sceneList = string.Join("\n", EditorBuildSettings.scenes.Select(x => "\"" + x.path + "\","));
                if(sceneList.Length==0)
                {
                    return;
                }
                sceneList += Environment.NewLine;

                Debug.Log(sceneList.ToString());
            }
            //if (GUILayout.Button("Open Buildscript"))
            //{
            //    System.Diagnostics.Process.Start(jsonPath);
            //}
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Build environments.
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

            string group = "";
            foreach (var bs in buildSettings.build_settings)
            {
                string name = bs.name;

                int index = bs.name.LastIndexOf('/');
                if (index!=-1)
                {
                    string g = bs.name.Substring(0, index);
                    name = bs.name.Substring(index+1);
                    if (group != g)
                    {
                        group = g;
                        GUILayout.Label(group);
                    }
                }

                if (name[0] != '.')
                {
                    bool activeEnv = UnitySettingSwitcherWindow.buildSettingsSelector.IsSelected(bs.name);
                    GUI.backgroundColor = activeEnv ? Color.gray : new Color(32, 96, 128);

                    if (GUILayout.Button((activeEnv ? "*":"") + name))
                    {
                        UnitySettingSwitcherWindow.buildSettingsSelector.LoadBuildSettingsSelected();
                        UnitySettingSwitcherWindow.buildSettingsSelector.Select(bs.name);
                        UnitySettingSwitcherWindow.buildSettingsSelector.SaveBuildSettingsSelected();

                        string[] envlist = UnitySettingSwitcherWindow.buildSettingsSelector.buildSettingsSelected.environmentPaths.ToArray();
                        Debug.Log("ApplyEnv: " + String.Join(" + ", envlist));
                        var env = SettingsUtil.Create(buildSettings, envlist);
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
            UnitySettingSwitcherWindow.buildSettings = buildSettings;
            Debug.Log("reload " + SettingConstants.SETTING_FILE_NAME + " finished");
        }

    }

}
