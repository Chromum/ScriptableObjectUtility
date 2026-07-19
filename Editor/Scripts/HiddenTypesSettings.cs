using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class HiddenTypesSettings
    {
        const string HiddenTypesKey = "Chromum.ScriptableObjectUtility.HiddenTypes";

        public static event Action HiddenTypesChanged;

        static HashSet<string> hiddenTypes;

        static HashSet<string> HiddenTypes
        {
            get
            {
                if (hiddenTypes == null)
                    hiddenTypes = Load();
                return hiddenTypes;
            }
        }

        public static bool IsHidden(Type type)
        {
            return HiddenTypes.Contains(type.AssemblyQualifiedName);
        }

        public static void SetHidden(Type type, bool hidden)
        {
            string key = type.AssemblyQualifiedName;
            bool changed = hidden ? HiddenTypes.Add(key) : HiddenTypes.Remove(key);

            if (!changed)
                return;

            EditorPrefs.SetString(HiddenTypesKey, string.Join(";", HiddenTypes));
            HiddenTypesChanged?.Invoke();
        }

        public static IEnumerable<string> GetAllHiddenKeys()
        {
            return HiddenTypes.ToList();
        }

        public static void SetHiddenByKey(string assemblyQualifiedName, bool hidden)
        {
            bool changed = hidden ? HiddenTypes.Add(assemblyQualifiedName) : HiddenTypes.Remove(assemblyQualifiedName);

            if (!changed)
                return;

            EditorPrefs.SetString(HiddenTypesKey, string.Join(";", HiddenTypes));
            HiddenTypesChanged?.Invoke();
        }

        static HashSet<string> Load()
        {
            string raw = EditorPrefs.GetString(HiddenTypesKey, string.Empty);
            return new HashSet<string>(raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static void ResetAll()
        {
            EditorPrefs.DeleteKey(HiddenTypesKey);
            hiddenTypes = null;
            HiddenTypesChanged?.Invoke();
        }
    }
}
