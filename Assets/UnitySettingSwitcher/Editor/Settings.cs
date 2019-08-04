using System;
using System.Collections.Generic;

namespace uisawara
{

    [Serializable]
    public class Settings
    {
        public List<Environment> build_settings = new List<Environment>();

        [Serializable]
        public class Environment
        {
            public string name;
            public string inherit;
            public List<string> scene_list;
            public KeyValueDictionary build_settings = new KeyValueDictionary();
            public KeyValueDictionary player_settings = new KeyValueDictionary();
            public KeyValueDictionary xr_settings = new KeyValueDictionary();
            public string scripting_define_symbols;
        }

        public int GetEnvironmentIndex(string environmentName)
        {
            int index = 0;
            foreach (var env in build_settings)
            {
                if (env.name == environmentName)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public Environment FindEnvironment(string environmentName)
        {
            int index = GetEnvironmentIndex(environmentName);
            if(index==-1)
            {
                return null;
            }
            return build_settings[index];
        }

        public string[] ResolveInherit(string environmentName)
        {
            var result = new List<string>();
            var current = environmentName;
            while (current != null)
            {
                result.Add(current);

                var env = FindEnvironment(current);
                if(env==null)
                {
                    throw new ArgumentNullException("Inherit setting not found: " + current);
                }

                current = env.inherit;
            }
            return result.ToArray();
        }

        public static Settings.Environment Merge(Settings.Environment lhs, Settings.Environment rhs)
        {
            var result = SettingsUtil.Copy(lhs);

            if (rhs.build_settings != null) result.build_settings.Hang(rhs.build_settings);
            if (rhs.player_settings != null) result.player_settings.Hang(rhs.player_settings);
            if (rhs.xr_settings != null) result.xr_settings.Hang(rhs.xr_settings);
            if (!string.IsNullOrEmpty(rhs.scripting_define_symbols)) result.scripting_define_symbols = result.scripting_define_symbols + ";" + rhs.scripting_define_symbols;
            if (rhs.scene_list != null) result.scene_list.AddRange(rhs.scene_list);

            return result;
        }

    }

}
