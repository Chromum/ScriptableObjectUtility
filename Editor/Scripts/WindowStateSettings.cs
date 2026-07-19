using UnityEditor;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal enum ToolViewMode
    {
        Tree = 0,
        AssetBrowser = 1,
    }

    internal static class WindowStateSettings
    {
        const string LastSearchQueryKey = "Chromum.ScriptableObjectUtility.LastSearchQuery";
        const string LastViewModeKey = "Chromum.ScriptableObjectUtility.LastViewMode";
        const string LastBrowsedTypeKey = "Chromum.ScriptableObjectUtility.LastBrowsedType";

        public static string LastSearchQuery
        {
            get => EditorPrefs.GetString(LastSearchQueryKey, string.Empty);
            set => EditorPrefs.SetString(LastSearchQueryKey, value);
        }

        public static ToolViewMode LastViewMode
        {
            get => (ToolViewMode)EditorPrefs.GetInt(LastViewModeKey, (int)ToolViewMode.Tree);
            set => EditorPrefs.SetInt(LastViewModeKey, (int)value);
        }

        public static string LastBrowsedTypeName
        {
            get => EditorPrefs.GetString(LastBrowsedTypeKey, string.Empty);
            set => EditorPrefs.SetString(LastBrowsedTypeKey, value);
        }

        public static void ResetAll()
        {
            EditorPrefs.DeleteKey(LastSearchQueryKey);
            EditorPrefs.DeleteKey(LastViewModeKey);
            EditorPrefs.DeleteKey(LastBrowsedTypeKey);
        }
    }
}
