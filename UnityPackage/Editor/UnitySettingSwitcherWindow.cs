using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

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

        private static Settings settings;
        private static SettingsSelector settingsSelector = new SettingsSelector();

        [MenuItem("Window/Unity Setting Switcher %e")]
        static void Open()
        {
            GetWindow<UnitySettingSwitcherWindow>(false, "Unity Setting Switcher");
        }

        void OnGUI()
        {

            string jsonPath = Path.Combine(Application.dataPath, SettingConstants.SETTING_FILE_NAME);
            settingsSelector.LoadBuildSettingsSelected();

            if (UnitySettingSwitcherWindow.settings == null)
            {
                Reload();
                if (settings == null)
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
                var sceneList = "";
                for(int i= 0;i<EditorSceneManager.sceneCount;i++)
                {
                    var x = EditorSceneManager.GetSceneAt(i).path;
                    sceneList += "\"" + x + "\"," +"\n";
                }
                //var sceneList = string.Join("\n", EditorBuildSettings.scenes.Select(x => "\"" + x.path + "\","));
                if (sceneList.Length==0)
                {
                    return;
                }
                sceneList += Environment.NewLine;

                Debug.Log(sceneList.ToString());
            }
            if (GUILayout.Button("Open Buildscript"))
            {
                System.Diagnostics.Process.Start(jsonPath);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Build environments
            GUILayout.BeginHorizontal();
            GUILayout.Label("Settings:");
            if (GUILayout.Button("Reload"))
            {
                Reload();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Setting buttons
            string group = "";
            foreach (var bs in settings.settings)
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
                    bool activeEnv = UnitySettingSwitcherWindow.settingsSelector.IsSelected(bs.name);
                    GUI.backgroundColor = activeEnv ? Color.gray : new Color(32, 96, 128);

                    if (GUILayout.Button((activeEnv ? "*":"") + name))
                    {
                        UnitySettingSwitcherWindow.settingsSelector.LoadBuildSettingsSelected();
                        UnitySettingSwitcherWindow.settingsSelector.Select(bs.name);
                        UnitySettingSwitcherWindow.settingsSelector.SaveBuildSettingsSelected();

                        string[] envlist = UnitySettingSwitcherWindow.settingsSelector.buildSettingsSelected.environmentPaths.ToArray();
                        Debug.Log("ApplyEnv: " + String.Join(" + ", envlist));
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
            UnitySettingSwitcherWindow.settings = buildSettings;
            Debug.Log("reload " + SettingConstants.SETTING_FILE_NAME + " finished");
        }

    }

}
