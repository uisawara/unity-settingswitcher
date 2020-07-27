using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace uisawara
{
    [Serializable]
    public class SettingsSelected
    {
        public List<string> environmentPaths = new List<string>();
    }

    public class SettingsSelector
    {
        public SettingsSelected buildSettingsSelected { get; private set; }

        public void LoadBuildSettingsSelected()
        {
            var jsonPath = Path.Combine(Application.dataPath, SettingConstants.SETTINGCURRENT_FILE_NAME);
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                var data = JsonUtility.FromJson<SettingsSelected>(json);
                buildSettingsSelected = data;
            }
            else
            {
                buildSettingsSelected = new SettingsSelected();
            }
        }

        public void SaveBuildSettingsSelected()
        {
            var jsonPath = Path.Combine(Application.dataPath, SettingConstants.SETTINGCURRENT_FILE_NAME);
            var json = JsonUtility.ToJson(buildSettingsSelected);
            File.WriteAllText(jsonPath, json);
        }

        public void Select(string envname)
        {
            // 既存設定をパース
            var pnl = new Dictionary<string, KeyValue<string, string>>();
            foreach (var ep in buildSettingsSelected.environmentPaths)
            {
                var eppn = SplitPathName(ep);
                pnl[eppn.key] = new KeyValue<string, string>
                {
                    key = ep,
                    value = eppn.value
                };
            }

            // 設定を更新
            var pn = SplitPathName(envname);
            if (pnl.ContainsKey(pn.key) && pnl[pn.key].key == envname)
                // 既存で有効であれば無効に切り替える
                pnl.Remove(pn.key);
            else
                // 設定を有効化
                pnl[pn.key] = new KeyValue<string, string>
                {
                    key = envname,
                    value = pn.value
                };

            // 新規設定を反映
            buildSettingsSelected.environmentPaths = pnl.Select(x => x.Value.key).ToList();
        }

        public bool IsSelected(string envname)
        {
            if (buildSettingsSelected == null) return false;
            return buildSettingsSelected.environmentPaths.IndexOf(envname) != -1;
        }

        /// <summary>
        ///     環境設定フルネームをグループ名と個別名に分割.
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private KeyValue<string, string> SplitPathName(string pathname)
        {
            var path = "";
            var name = "";

            var index = pathname.LastIndexOf('/');
            if (index == -1)
            {
                name = pathname;
            }
            else
            {
                path = pathname.Substring(0, index);
                name = pathname.Substring(index + 1);
            }

            var result = new KeyValue<string, string>
            {
                key = path,
                value = name
            };
            return result;
        }

        private class KeyValue<TKey, TValue>
        {
            public TKey key;
            public TValue value;
        }
    }
}