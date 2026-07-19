using UnityEditor;

namespace Bonejam.ScriptableObjectUtility.Editor
{
    internal static class FoldoutExpandStateStore
    {
        const string KeyPrefix = "Bonejam.ScriptableObjectUtility.Foldout.";

        public static bool SuppressWrites;

        public static bool GetExpanded(string path) => EditorPrefs.GetBool(KeyPrefix + path, true);

        public static void SetExpanded(string path, bool expanded)
        {
            if (SuppressWrites)
                return;

            EditorPrefs.SetBool(KeyPrefix + path, expanded);
        }
    }
}
