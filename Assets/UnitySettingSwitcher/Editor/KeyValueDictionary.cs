using System;
using System.Collections.Generic;
using UnityEngine;

namespace uisawara
{

    [Serializable]
    public class KeyValueDictionary : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<KeyValue> keyValues = new List<KeyValue>();

        public Dictionary<string, ValueSet> dictionary = new Dictionary<string, ValueSet>();

        public void Hang(KeyValueDictionary target)
        {
            foreach (var t in target.dictionary)
            {
                if (dictionary.ContainsKey(t.Key))
                {
                    continue;
                }

                dictionary[t.Key] = t.Value;
            }
        }

        public void OnBeforeSerialize()
        {
            keyValues.Clear();

            foreach (var kv in dictionary)
            {
                keyValues.Add(new KeyValue()
                {
                    key = kv.Key,
                    s = kv.Value.s,
                    b = kv.Value.b,
                    i = kv.Value.i,
                    f = kv.Value.f,
                });
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();

            foreach (var kv in keyValues)
            {
                dictionary.Add(kv.key, (ValueSet)kv);
            }
        }

        [Serializable]
        public class KeyValue : ValueSet
        {
            public string key;
        }

    }

    [Serializable]
    public class ValueSet
    {
        public string s;
        public bool b;
        public int i;
        public float f;
    }

}
