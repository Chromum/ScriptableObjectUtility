using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal class PathFoldoutTree
    {
        const string EditScriptIconName = "d_editicon.sml";
        const string ShowAssetsIconName = "Search Icon";

        static readonly MethodInfo AddButtonMethod = typeof(PathFoldoutTree).GetMethod(nameof(AddButton));
        static readonly MethodInfo GetActiveFolderPathMethod =
            typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        public readonly string fullPath;
        public readonly Foldout foldout;
        public readonly Dictionary<string, PathFoldoutTree> subDirectories = new Dictionary<string, PathFoldoutTree>();

        readonly VisualTreeAsset buttonAsset;
        readonly Action<Type> onShowAssetsRequested;
        readonly Dictionary<string, VisualElement> buttonRows = new Dictionary<string, VisualElement>();
        readonly Dictionary<string, string> buttonTypeFullNames = new Dictionary<string, string>();

        public PathFoldoutTree(VisualElement parent, string name, string fullPath, VisualTreeAsset buttonAsset, VisualTreeAsset foldoutAsset, Action<Type> onShowAssetsRequested)
        {
            this.fullPath = fullPath;
            this.buttonAsset = buttonAsset;
            this.onShowAssetsRequested = onShowAssetsRequested;

            var instance = foldoutAsset.Instantiate();
            foldout = instance.Q<Foldout>("foldout-root");
            foldout.text = name;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            foldout.value = FoldoutExpandStateStore.GetExpanded(fullPath);
            foldout.RegisterValueChangedCallback(evt => FoldoutExpandStateStore.SetExpanded(fullPath, evt.newValue));

            parent.Add(foldout);
        }

        public void AddButton<T>(string label, int assetCount) where T : ScriptableObject
        {
            if (buttonRows.ContainsKey(label))
                return;

            var instance = buttonAsset.Instantiate();
            var optionsButton = instance.Q<Button>("optionsBttn");
            var button = instance.Q<Button>("namedBttn");
            var editScriptButton = instance.Q<Button>("editScriptBttn");
            var showAssetsButton = instance.Q<Button>("showAssetsBttn");

            button.text = ToolPreferences.ShowAssetCountBadge ? $"{label} ({assetCount})" : label;
            button.tooltip = typeof(T).FullName;

            button.clicked += () => CreateAsset<T>();

            optionsButton.tooltip = "Options";
            optionsButton.clicked += () => ShowOptionsMenu<T>(optionsButton);

            EditorIconUtility.SetIcon(editScriptButton, EditScriptIconName, "Edit Script");
            editScriptButton.clicked += () => EditScript(typeof(T));
            editScriptButton.style.display = ToolPreferences.ShowEditScriptButton ? DisplayStyle.Flex : DisplayStyle.None;

            EditorIconUtility.SetIcon(showAssetsButton, ShowAssetsIconName, "Show All Assets");
            showAssetsButton.clicked += () => onShowAssetsRequested?.Invoke(typeof(T));
            showAssetsButton.style.display = ToolPreferences.ShowShowAssetsButton ? DisplayStyle.Flex : DisplayStyle.None;

            foldout.Add(instance);
            ReorderFoldoutChildren(foldout);

            buttonRows.Add(label, instance);
            buttonTypeFullNames.Add(label, typeof(T).FullName);
        }

        public static void AddButtonReflective(PathFoldoutTree tree, Type type, string label, int assetCount)
        {
            var genericMethod = AddButtonMethod.MakeGenericMethod(type);
            genericMethod.Invoke(tree, new object[] { label, assetCount });
        }

        public bool ApplyFilter(string query, bool hasQuery)
        {
            bool anyVisible = false;

            foreach (var kvp in buttonRows)
            {
                string label = kvp.Key;
                bool isMatch = !hasQuery
                    || label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                    || buttonTypeFullNames[label].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

                kvp.Value.style.display = isMatch ? DisplayStyle.Flex : DisplayStyle.None;
                anyVisible |= isMatch;
            }

            foreach (var child in subDirectories.Values)
                anyVisible |= child.ApplyFilter(query, hasQuery);

            foldout.style.display = (!hasQuery || anyVisible) ? DisplayStyle.Flex : DisplayStyle.None;

            FoldoutExpandStateStore.SuppressWrites = true;
            foldout.value = hasQuery ? anyVisible : FoldoutExpandStateStore.GetExpanded(fullPath);
            FoldoutExpandStateStore.SuppressWrites = false;

            return anyVisible;
        }

        static void CreateAsset<T>() where T : ScriptableObject
        {
            string targetFolder = ResolveTargetFolder<T>();
            if (targetFolder == null)
                return;

            var createdSO = ScriptableObject.CreateInstance<T>();
            createdSO.name = $"New {typeof(T).Name}";

            ProjectWindowUtil.CreateAsset(createdSO, $"{targetFolder}/{createdSO.name}.asset");
            Undo.RegisterCreatedObjectUndo(createdSO, $"Create {typeof(T).Name}");
        }

        static string ResolveTargetFolder<T>() where T : ScriptableObject
        {
            switch (ToolPreferences.CreationFolderMode)
            {
                case AssetCreationFolderMode.MostCommonFolderForType:
                    return ResolveMostCommonFolder<T>() ?? GetActiveFolderPath();

                case AssetCreationFolderMode.FixedDefaultFolder:
                    return ToolPreferences.FixedDefaultFolder;

                case AssetCreationFolderMode.PromptEachTime:
                    string chosen = EditorUtility.SaveFolderPanel($"Choose Folder For New {typeof(T).Name}", "Assets", string.Empty);
                    return string.IsNullOrEmpty(chosen) ? null : MakeProjectRelative(chosen);

                default:
                    return GetActiveFolderPath();
            }
        }

        static string ResolveMostCommonFolder<T>() where T : ScriptableObject
        {
            var existing = ScriptableObjectAssetFinder.FindAssetsOfExactType(typeof(T));
            if (existing.Count == 0)
                return null;

            return existing
                .Select(a => System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(a)).Replace('\\', '/'))
                .GroupBy(p => p)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        static string GetActiveFolderPath()
        {
            return (string)GetActiveFolderPathMethod.Invoke(null, null);
        }

        static string MakeProjectRelative(string absolutePath)
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath)?.Replace('\\', '/');
            absolutePath = absolutePath.Replace('\\', '/');

            if (projectRoot != null && absolutePath.StartsWith(projectRoot))
                return absolutePath.Substring(projectRoot.Length + 1);

            Debug.LogWarning($"Chosen folder \"{absolutePath}\" is outside the project — defaulting to Assets.");
            return "Assets";
        }

        static void ShowOptionsMenu<T>(VisualElement anchor) where T : ScriptableObject
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Hide This Type"), false, () => HiddenTypesSettings.SetHidden(typeof(T), true));
            menu.DropDown(anchor.worldBound);
        }

        static void EditScript(Type type)
        {
            var script = ScriptableObjectAssetFinder.FindScriptForType(type);
            if (script != null)
                AssetDatabase.OpenAsset(script);
            else
                Debug.LogWarning($"Could not find a script asset for {type.FullName}.");
        }

        static void ReorderFoldoutChildren(Foldout parentFoldout)
        {
            var foldoutChildren = new List<VisualElement>();
            var otherChildren = new List<VisualElement>();

            foreach (var child in parentFoldout.Children())
            {
                if (child is Foldout)
                    foldoutChildren.Add(child);
                else
                    otherChildren.Add(child);
            }

            parentFoldout.Clear();

            foreach (var child in foldoutChildren)
                parentFoldout.Add(child);
            foreach (var child in otherChildren)
                parentFoldout.Add(child);
        }
    }
}
