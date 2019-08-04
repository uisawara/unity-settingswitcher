using System;
using System.Collections.Generic;

namespace uisawara
{

    public class Settings
    {
        public List<Environment> settings = new List<Environment>();

        public class Environment
        {
            public string name;
            public string inherit;
            public List<string> scene_list;
            public Dictionary<string, object> build_settings = new Dictionary<string, object>();
            public Dictionary<string, object> player_settings = new Dictionary<string, object>();
            public Dictionary<string, object> xr_settings = new Dictionary<string, object>();
            public string scripting_define_symbols;
        }

        public int GetEnvironmentIndex(string environmentName)
        {
            int index = 0;
            foreach (var env in settings)
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
            return settings[index];
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

        public static Environment Merge(Settings.Environment lhs, Settings.Environment rhs)
        {
            var result = SettingsUtil.Copy(lhs);

            // Merge settings
            if (rhs.build_settings != null) Settings.Merge(result.build_settings, rhs.build_settings);
            if (rhs.player_settings != null) Settings.Merge(result.player_settings, rhs.player_settings);
            if (rhs.xr_settings != null) Settings.Merge(result.xr_settings, rhs.xr_settings);

            // Special merge settings
            if (!string.IsNullOrEmpty(rhs.scripting_define_symbols)) result.scripting_define_symbols = result.scripting_define_symbols + ";" + rhs.scripting_define_symbols;
            if (rhs.scene_list != null) result.scene_list.AddRange(rhs.scene_list);

            return result;
        }

        /// <summary>
        /// Merge rhs settings to lhs settings
        /// </summary>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        public static void Merge(Dictionary<string, object> lhs, Dictionary<string, object> rhs)
        {
            foreach (var kv in rhs)
            {
                if (lhs.ContainsKey(kv.Key))
                {
                    continue;
                }

                lhs[kv.Key] = kv.Value;
            }
        }

    }

}
