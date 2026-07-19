using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chromum.ScriptableObjectUtility.Editor
{
    public class ScriptableObjectUtilityWindow : EditorWindow, IHasCustomMenu
    {
        const string PackageUxmlRoot = "Packages/com.chromum.scriptable-object-utility/Editor/UXML";
        const string PackageUssRoot = "Packages/com.chromum.scriptable-object-utility/Editor/USS";

        readonly Dictionary<string, PathFoldoutTree> rootFoldouts = new Dictionary<string, PathFoldoutTree>();

        Foldout foldoutsRoot;
        ToolbarSearchField searchField;
        VisualTreeAsset windowAsset, buttonAsset, foldoutAsset;

        ScrollView treeScrollView;
        ScrollView assetListScrollView;
        VisualElement assetBrowserHeader;
        VisualElement assetListContainer;
        Label assetBrowserTitle;
        Button backButton;

        VisualElement inspectorContainer;

        [MenuItem("Window/Scriptable Object Utility")]
        public static void ShowWindow()
        {
            var window = GetWindow<ScriptableObjectUtilityWindow>();
            window.titleContent = new GUIContent("Scriptable Object Utility");
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Refresh ScriptableObject List"), false, PopulateTree);
            menu.AddItem(new GUIContent("Manage Preferences..."), false,
                () => SettingsService.OpenUserPreferences(ScriptableObjectUtilityPreferences.SettingsPath));
        }

        public void CreateGUI()
        {
            windowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PackageUxmlRoot}/ScriptableObjectUtilityWindow.uxml");
            buttonAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PackageUxmlRoot}/SOButton.uxml");
            foldoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PackageUxmlRoot}/FoldoutTemplate.uxml");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{PackageUssRoot}/ScriptableObjectUtilityWindow.uss");
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"Could not load stylesheet at {PackageUssRoot}/ScriptableObjectUtilityWindow.uss");

            rootVisualElement.Add(windowAsset.Instantiate());

            var topPane = rootVisualElement.Q<VisualElement>("top-pane");
            var bottomPane = rootVisualElement.Q<VisualElement>("bottom-pane");
            topPane.RemoveFromHierarchy();
            bottomPane.RemoveFromHierarchy();

            var splitView = new TwoPaneSplitView(1, 220, TwoPaneSplitViewOrientation.Vertical) { style = { flexGrow = 1 } };
            splitView.Add(topPane);
            splitView.Add(bottomPane);
            rootVisualElement.Add(splitView);

            foldoutsRoot = rootVisualElement.Q<Foldout>("foldouts-root");
            searchField = rootVisualElement.Q<ToolbarSearchField>("so-search-field");
            searchField.RegisterValueChangedCallback(evt =>
            {
                ApplyFilter(evt.newValue);
                WindowStateSettings.LastSearchQuery = evt.newValue;
            });

            treeScrollView = rootVisualElement.Q<ScrollView>("tree-scroll-view");
            assetListScrollView = rootVisualElement.Q<ScrollView>("asset-list-scroll-view");
            assetBrowserHeader = rootVisualElement.Q<VisualElement>("asset-browser-header");
            assetListContainer = rootVisualElement.Q<VisualElement>("asset-list-container");
            assetBrowserTitle = rootVisualElement.Q<Label>("asset-browser-title");
            backButton = rootVisualElement.Q<Button>("back-button");
            backButton.clicked += ShowTreeView;

            inspectorContainer = rootVisualElement.Q<VisualElement>("inspector-container");
            ShowInspector(null);

            AssemblyFilterSettings.IncludedAssembliesChanged += PopulateTree;
            HiddenTypesSettings.HiddenTypesChanged += PopulateTree;
            ToolPreferences.DisplayPreferencesChanged += PopulateTree;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            searchField.value = WindowStateSettings.LastSearchQuery;

            PopulateTree();
            RestoreLastView();
        }

        void OnDisable()
        {
            AssemblyFilterSettings.IncludedAssembliesChanged -= PopulateTree;
            HiddenTypesSettings.HiddenTypesChanged -= PopulateTree;
            ToolPreferences.DisplayPreferencesChanged -= PopulateTree;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        void OnAfterAssemblyReload()
        {
            if (ToolPreferences.AutoRefreshOnCompile)
                PopulateTree();
        }

        void RestoreLastView()
        {
            if (WindowStateSettings.LastViewMode != ToolViewMode.AssetBrowser)
                return;

            var type = Type.GetType(WindowStateSettings.LastBrowsedTypeName);
            if (type != null)
                ShowAssetBrowser(type);
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
                    next = new PathFoldoutTree(parentElement, segment, cumulativePath, buttonAsset, foldoutAsset, ShowAssetBrowser);
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

            int assetCount = ScriptableObjectAssetFinder.FindAssetsOfExactType(type).Count;
            PathFoldoutTree.AddButtonReflective(current, type, displayName, assetCount);
        }

        void ApplyFilter(string query)
        {
            bool hasQuery = !string.IsNullOrWhiteSpace(query);
            foreach (var root in rootFoldouts.Values)
                root.ApplyFilter(query, hasQuery);
        }

        void ShowAssetBrowser(Type type)
        {
            assetBrowserHeader.style.display = DisplayStyle.Flex;
            treeScrollView.style.display = DisplayStyle.None;
            assetListScrollView.style.display = DisplayStyle.Flex;
            assetBrowserTitle.text = $"Assets of type: {type.Name}";

            WindowStateSettings.LastViewMode = ToolViewMode.AssetBrowser;
            WindowStateSettings.LastBrowsedTypeName = type.AssemblyQualifiedName;

            assetListContainer.Clear();
            var assets = ScriptableObjectAssetFinder.FindAssetsOfExactType(type);

            if (assets.Count == 0)
            {
                var emptyLabel = new Label("No assets of this type exist yet.");
                emptyLabel.AddToClassList("so-empty-label");
                assetListContainer.Add(emptyLabel);
                return;
            }

            for (int i = 0; i < assets.Count; i++)
                assetListContainer.Add(BuildAssetRow(assets[i], i));
        }

        VisualElement BuildAssetRow(ScriptableObject asset, int index)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            row.AddToClassList("so-asset-row");
            if (index % 2 == 1)
                row.AddToClassList("so-asset-row-alt");

            var previewButton = new Button(() => ShowInspector(asset))
            {
                text = asset.name,
                style = { flexGrow = 1, height = 22 }
            };
            previewButton.AddToClassList("so-row-button");
            row.Add(previewButton);

            var selectButton = new Button(() =>
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            });
            selectButton.AddToClassList("so-icon-button");
            EditorIconUtility.SetIcon(selectButton, "d_Folder Icon", "Select In Project");
            row.Add(selectButton);

            return row;
        }

        void ShowTreeView()
        {
            assetBrowserHeader.style.display = DisplayStyle.None;
            treeScrollView.style.display = DisplayStyle.Flex;
            assetListScrollView.style.display = DisplayStyle.None;

            WindowStateSettings.LastViewMode = ToolViewMode.Tree;

            ShowInspector(null);
        }

        void ShowInspector(UnityEngine.Object target)
        {
            inspectorContainer.Clear();

            if (target == null)
            {
                var placeholder = new Label("Select an asset above to preview it here.");
                placeholder.AddToClassList("so-empty-label");
                inspectorContainer.Add(placeholder);
                return;
            }

            inspectorContainer.Add(new InspectorElement(target));
        }
    }
}
