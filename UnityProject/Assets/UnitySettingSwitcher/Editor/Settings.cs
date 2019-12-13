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
            public List<string> scene_list = new List<string>();

            // TODO You add setting item to here
            public Dictionary<string, object> build_settings = new Dictionary<string, object>();
            public Dictionary<string, object> player_settings = new Dictionary<string, object>();
            public Dictionary<string, object> editor_user_build_settings = new Dictionary<string, object>();
            public Dictionary<string, object> xr_settings = new Dictionary<string, object>();
            public Dictionary<string, object> android = new Dictionary<string, object>();
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

        /// <summary>
        /// Merge rhs settings to lhs settings
        /// </summary>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        public static void Merge(Settings.Environment lhs, Settings.Environment rhs)
        {
            // Special merge settings

            if (rhs.player_settings != null && rhs.player_settings.ContainsKey("scripting_define_symbols"))
            {
                string v;
                if(lhs.player_settings==null)
                {
                    lhs.player_settings = new Dictionary<string, object>();
                }
                if (lhs.player_settings.ContainsKey("scripting_define_symbols"))
                {
                    v = lhs.player_settings["scripting_define_symbols"] + ";";
                } else
                {
                    v = "";
                }
                lhs.player_settings["scripting_define_symbols"] = v + rhs.player_settings["scripting_define_symbols"];
            }

            if (rhs.scene_list != null) lhs.scene_list.AddRange(rhs.scene_list);

            // TODO If you add setting item then add process to here
            // Merge settings
            if (rhs.build_settings != null) Settings.Merge(lhs.build_settings, rhs.build_settings);
            if (rhs.player_settings != null) Settings.Merge(lhs.player_settings, rhs.player_settings);
            if (rhs.editor_user_build_settings != null) Settings.Merge(lhs.editor_user_build_settings, rhs.editor_user_build_settings);
            if (rhs.xr_settings != null) Settings.Merge(lhs.xr_settings, rhs.xr_settings);
            if (rhs.android != null) Settings.Merge(lhs.android, rhs.android);

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
