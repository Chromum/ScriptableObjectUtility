using System;
using UnityEditor;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal enum AssetCreationFolderMode
    {
        ActiveProjectFolder = 0,
        MostCommonFolderForType = 1,
        FixedDefaultFolder = 2,
        PromptEachTime = 3,
    }

    internal static class ToolPreferences
    {
        const string CreationFolderModeKey = "Chromum.ScriptableObjectUtility.CreationFolderMode";
        const string FixedDefaultFolderKey = "Chromum.ScriptableObjectUtility.FixedDefaultFolder";
        const string DefaultFoldoutExpandedKey = "Chromum.ScriptableObjectUtility.DefaultFoldoutExpanded";
        const string AutoRefreshOnCompileKey = "Chromum.ScriptableObjectUtility.AutoRefreshOnCompile";
        const string ShowAssetCountBadgeKey = "Chromum.ScriptableObjectUtility.ShowAssetCountBadge";
        const string ShowEditScriptButtonKey = "Chromum.ScriptableObjectUtility.ShowEditScriptButton";
        const string ShowShowAssetsButtonKey = "Chromum.ScriptableObjectUtility.ShowShowAssetsButton";

        public static event Action DisplayPreferencesChanged;

        public static AssetCreationFolderMode CreationFolderMode
        {
            get => (AssetCreationFolderMode)EditorPrefs.GetInt(CreationFolderModeKey, (int)AssetCreationFolderMode.ActiveProjectFolder);
            set => EditorPrefs.SetInt(CreationFolderModeKey, (int)value);
        }

        public static string FixedDefaultFolder
        {
            get => EditorPrefs.GetString(FixedDefaultFolderKey, "Assets");
            set => EditorPrefs.SetString(FixedDefaultFolderKey, value);
        }

        public static bool DefaultFoldoutExpanded
        {
            get => EditorPrefs.GetBool(DefaultFoldoutExpandedKey, true);
            set => EditorPrefs.SetBool(DefaultFoldoutExpandedKey, value);
        }

        public static bool AutoRefreshOnCompile
        {
            get => EditorPrefs.GetBool(AutoRefreshOnCompileKey, true);
            set => EditorPrefs.SetBool(AutoRefreshOnCompileKey, value);
        }

        public static bool ShowAssetCountBadge
        {
            get => EditorPrefs.GetBool(ShowAssetCountBadgeKey, true);
            set
            {
                if (value == ShowAssetCountBadge)
                    return;
                EditorPrefs.SetBool(ShowAssetCountBadgeKey, value);
                DisplayPreferencesChanged?.Invoke();
            }
        }

        public static bool ShowEditScriptButton
        {
            get => EditorPrefs.GetBool(ShowEditScriptButtonKey, true);
            set
            {
                if (value == ShowEditScriptButton)
                    return;
                EditorPrefs.SetBool(ShowEditScriptButtonKey, value);
                DisplayPreferencesChanged?.Invoke();
            }
        }

        public static bool ShowShowAssetsButton
        {
            get => EditorPrefs.GetBool(ShowShowAssetsButtonKey, true);
            set
            {
                if (value == ShowShowAssetsButton)
                    return;
                EditorPrefs.SetBool(ShowShowAssetsButtonKey, value);
                DisplayPreferencesChanged?.Invoke();
            }
        }

        public static void ResetAll()
        {
            EditorPrefs.DeleteKey(CreationFolderModeKey);
            EditorPrefs.DeleteKey(FixedDefaultFolderKey);
            EditorPrefs.DeleteKey(DefaultFoldoutExpandedKey);
            EditorPrefs.DeleteKey(AutoRefreshOnCompileKey);
            EditorPrefs.DeleteKey(ShowAssetCountBadgeKey);
            EditorPrefs.DeleteKey(ShowEditScriptButtonKey);
            EditorPrefs.DeleteKey(ShowShowAssetsButtonKey);
            DisplayPreferencesChanged?.Invoke();
        }
    }
}
