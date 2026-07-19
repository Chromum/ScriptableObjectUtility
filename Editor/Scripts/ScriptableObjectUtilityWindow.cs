using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bonejam.ScriptableObjectUtility.Editor
{
    public class ScriptableObjectUtilityWindow : EditorWindow, IHasCustomMenu
    {
        const string PackageUxmlRoot = "Packages/com.chromum.scriptable-object-utility/Editor/UXML";

        readonly Dictionary<string, PathFoldoutTree> rootFoldouts = new Dictionary<string, PathFoldoutTree>();

        Foldout foldoutsRoot;
        ToolbarSearchField searchField;
        VisualTreeAsset windowAsset, buttonAsset, foldoutAsset;

        [MenuItem("Window/Scriptable Object Utility")]
        public static void ShowWindow()
        {
            var window = GetWindow<ScriptableObjectUtilityWindow>();
            window.titleContent = new GUIContent("Scriptable Object Utility");
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Refresh ScriptableObject List"), false, PopulateTree);
        }

        public void CreateGUI()
        {
            windowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PackageUxmlRoot}/ScriptableObjectUtilityWindow.uxml");
            buttonAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PackageUxmlRoot}/SOButton.uxml");
            foldoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PackageUxmlRoot}/FoldoutTemplate.uxml");

            rootVisualElement.Add(windowAsset.Instantiate());

            foldoutsRoot = rootVisualElement.Q<Foldout>("foldouts-root");
            searchField = rootVisualElement.Q<ToolbarSearchField>("so-search-field");
            searchField.RegisterValueChangedCallback(evt => ApplyFilter(evt.newValue));

            PopulateTree();
        }

        void PopulateTree()
        {
            rootFoldouts.Clear();
            foldoutsRoot.Clear();

            var types = TypeDiscovery.DiscoverScriptableObjectTypes().ToList();
            TypeDiscovery.ValidateMisusedAttributes(types);

            foreach (var type in types)
            {
                string[] groupPath = TypeDiscovery.ResolveGroupPath(type, out string displayName);
                AddTypeToTree(type, groupPath, displayName);
            }

            ApplyFilter(searchField?.value);
        }

        void AddTypeToTree(Type type, string[] groupPath, string displayName)
        {
            var lookup = rootFoldouts;
            VisualElement parentElement = foldoutsRoot;
            PathFoldoutTree current = null;
            string cumulativePath = null;

            foreach (var segment in groupPath)
            {
                cumulativePath = cumulativePath == null ? segment : $"{cumulativePath}/{segment}";

                if (!lookup.TryGetValue(segment, out var next))
                {
                    next = new PathFoldoutTree(parentElement, segment, cumulativePath, buttonAsset, foldoutAsset);
                    lookup.Add(segment, next);
                }

                current = next;
                parentElement = current.foldout;
                lookup = current.subDirectories;
            }

            if (current == null)
            {
                Debug.LogWarning($"{type.FullName} could not be grouped and was skipped.");
                return;
            }

            PathFoldoutTree.AddButtonReflective(current, type, displayName);
        }

        void ApplyFilter(string query)
        {
            bool hasQuery = !string.IsNullOrWhiteSpace(query);
            foreach (var root in rootFoldouts.Values)
                root.ApplyFilter(query, hasQuery);
        }
    }
}
