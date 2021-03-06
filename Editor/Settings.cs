﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace uisawara
{
    public class Settings
    {
        public List<Environment> settings = new List<Environment>();

        public int GetEnvironmentIndex(string environmentName)
        {
            var index = 0;
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
            var index = GetEnvironmentIndex(environmentName);
            if (index == -1)
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
                if (env == null)
                {
                    throw new ArgumentNullException("Inherit setting not found: " + current);
                }

                current = env.inherit;
            }

            return result.ToArray();
        }

        /// <summary>
        ///     Merge rhs settings to lhs settings
        /// </summary>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        public static void Merge(Environment lhs, Environment rhs)
        {
            // Special merge settings

            if (rhs.player_settings != null && rhs.player_settings.ContainsKey("scripting_define_symbols"))
            {
                string v;
                if (lhs.player_settings == null)
                {
                    lhs.player_settings = new Dictionary<string, object>();
                }

                if (lhs.player_settings.ContainsKey("scripting_define_symbols"))
                {
                    v = lhs.player_settings["scripting_define_symbols"] + ";";
                }
                else
                {
                    v = "";
                }

                lhs.player_settings["scripting_define_symbols"] = v + rhs.player_settings["scripting_define_symbols"];
            }

            if (rhs.editor_build_settings != null && rhs.editor_build_settings.ContainsKey("scenes"))
            {
                var items = (List<object>) rhs.editor_build_settings["scenes"];
                var scenes = items.Select(arg => (string) arg).ToArray();
                lhs.scene_list.AddRange(scenes);
            }

            // TODO If you add setting item then add process to here
            // Merge settings
            if (rhs.build_settings != null)
            {
                Merge(lhs.build_settings, rhs.build_settings);
            }

            if (rhs.player_settings != null)
            {
                Merge(lhs.player_settings, rhs.player_settings);
            }

            if (rhs.editor_build_settings != null)
            {
                Merge(lhs.editor_build_settings, rhs.editor_build_settings);
            }

            if (rhs.editor_user_build_settings != null)
            {
                Merge(lhs.editor_user_build_settings, rhs.editor_user_build_settings);
            }

            if (rhs.xr_settings != null)
            {
                Merge(lhs.xr_settings, rhs.xr_settings);
            }

            if (rhs.android != null)
            {
                Merge(lhs.android, rhs.android);
            }
        }

        /// <summary>
        ///     Merge rhs settings to lhs settings
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

        public class Environment
        {
            public Dictionary<string, object> android = new Dictionary<string, object>();

            // TODO You add setting item to here
            public Dictionary<string, object> build_settings = new Dictionary<string, object>();
            public Dictionary<string, object> editor_build_settings = new Dictionary<string, object>();
            public Dictionary<string, object> editor_user_build_settings = new Dictionary<string, object>();
            public string inherit;
            public string name;
            public Dictionary<string, object> player_settings = new Dictionary<string, object>();
            public List<string> scene_list = new List<string>();
            public Dictionary<string, object> xr_settings = new Dictionary<string, object>();
        }
    }
}