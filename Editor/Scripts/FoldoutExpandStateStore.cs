using UnityEditor;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class FoldoutExpandStateStore
    {
        const string KeyPrefix = "Chromum.ScriptableObjectUtility.Foldout.";

        public static bool SuppressWrites;

        public static bool GetExpanded(string path) => EditorPrefs.GetBool(KeyPrefix + path, ToolPreferences.DefaultFoldoutExpanded);

        public static void SetExpanded(string path, bool expanded)
        {
            if (SuppressWrites)
                return;

            EditorPrefs.SetBool(KeyPrefix + path, expanded);
        }
    }
}
