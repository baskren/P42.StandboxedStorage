using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SandboxedStorageSample
{
    public static class Preferences
    {

        static Dictionary<string, string> _backingStore;
        static Dictionary<string,string> BackingStore
        {
            get
            {
                if (_backingStore is null)
                {
                    if (P42.Utils.TextCache.Recall("Preferences.BackingStore") is string json)
                    {
                        _backingStore = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    }
                    else
                        _backingStore = new Dictionary<string, string>();
                }
                return _backingStore;
            }
        }

        static void SaveBackingStore()
        {
            var json = JsonConvert.SerializeObject(BackingStore);
            P42.Utils.TextCache.Store(json, "Preferences.BackingStore");
        }

        public static string Get(string key, string defaultValue)
        {
            if (BackingStore.TryGetValue(key, out string value))
                return value;
            return defaultValue;
        }

        public static void Set(string key, string value)
        {
            BackingStore[key] = value;
            SaveBackingStore();
        }

        public static void Clear(string key)
        {
            BackingStore.Remove(key);
            SaveBackingStore();
        }
    }
}
