using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class ScriptableObjectUtilityPreferences
    {
        public const string SettingsPath = "Preferences/Scriptable Object Utility";

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                label = "Scriptable Object Utility",
                activateHandler = OnActivate,
                keywords = new HashSet<string> { "ScriptableObject", "Assembly", "Included", "Utility" }
            };
        }

        static void OnActivate(string searchContext, VisualElement rootElement)
        {
            rootElement.style.paddingLeft = 10;
            rootElement.style.paddingRight = 10;
            rootElement.style.paddingTop = 8;
            rootElement.style.paddingBottom = 8;

            var refreshButton = new Button { text = "Refresh" };
            refreshButton.style.width = 80;
            refreshButton.style.marginBottom = 10;
            rootElement.Add(refreshButton);

            var assetCreationSection = BuildAssetCreationSection(out Action refreshAssetCreation);
            rootElement.Add(assetCreationSection);

            var windowBehaviorSection = BuildWindowBehaviorSection(out Action refreshWindowBehavior);
            rootElement.Add(windowBehaviorSection);

            var assembliesSection = BuildAssembliesSection(out Action refreshAssemblies);
            rootElement.Add(assembliesSection);

            var classesSection = BuildClassesSection(out Action refreshClasses);
            rootElement.Add(classesSection);

            var hiddenSection = BuildHiddenTypesSection(out Action refreshHidden);
            rootElement.Add(hiddenSection);

            refreshButton.clicked += () =>
            {
                refreshAssemblies();
                refreshClasses();
                refreshHidden();
            };

            var resetButton = new Button { text = "Reset All Settings to Defaults" };
            resetButton.style.marginTop = 20;
            resetButton.style.alignSelf = Align.FlexStart;
            resetButton.clicked += () =>
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Reset Scriptable Object Utility Settings",
                    "This resets asset-creation mode, window behavior toggles, included assemblies/classes, " +
                    "and hidden types back to their defaults. This cannot be undone.\n\n" +
                    "(Per-group foldout expand/collapse states you've already set are not affected.)",
                    "Reset", "Cancel");

                if (!confirmed)
                    return;

                ToolPreferences.ResetAll();
                AssemblyFilterSettings.ResetAll();
                HiddenTypesSettings.ResetAll();
                WindowStateSettings.ResetAll();

                refreshAssetCreation();
                refreshWindowBehavior();
                refreshAssemblies();
                refreshClasses();
                refreshHidden();
            };
            rootElement.Add(resetButton);
        }

        static VisualElement BuildWindowBehaviorSection(out Action refresh)
        {
            var section = new VisualElement { style = { marginTop = 16 } };
            section.Add(SectionHeader("Window Behavior"));

            var expandToggle = new Toggle("Start new groups expanded") { value = ToolPreferences.DefaultFoldoutExpanded };
            expandToggle.tooltip = "Applies to newly-discovered foldout groups only — groups you've already " +
                "expanded/collapsed keep their remembered state.";
            expandToggle.RegisterValueChangedCallback(evt => ToolPreferences.DefaultFoldoutExpanded = evt.newValue);

            var autoRefreshToggle = new Toggle("Auto-refresh after script compile") { value = ToolPreferences.AutoRefreshOnCompile };
            autoRefreshToggle.RegisterValueChangedCallback(evt => ToolPreferences.AutoRefreshOnCompile = evt.newValue);

            var countBadgeToggle = new Toggle("Show asset count badge") { value = ToolPreferences.ShowAssetCountBadge };
            countBadgeToggle.RegisterValueChangedCallback(evt => ToolPreferences.ShowAssetCountBadge = evt.newValue);

            var editScriptToggle = new Toggle("Show \"Edit Script\" button") { value = ToolPreferences.ShowEditScriptButton };
            editScriptToggle.RegisterValueChangedCallback(evt => ToolPreferences.ShowEditScriptButton = evt.newValue);

            var showAssetsToggle = new Toggle("Show \"Show All Assets\" button") { value = ToolPreferences.ShowShowAssetsButton };
            showAssetsToggle.RegisterValueChangedCallback(evt => ToolPreferences.ShowShowAssetsButton = evt.newValue);

            section.Add(expandToggle);
            section.Add(autoRefreshToggle);
            section.Add(countBadgeToggle);
            section.Add(editScriptToggle);
            section.Add(showAssetsToggle);

            refresh = () =>
            {
                expandToggle.SetValueWithoutNotify(ToolPreferences.DefaultFoldoutExpanded);
                autoRefreshToggle.SetValueWithoutNotify(ToolPreferences.AutoRefreshOnCompile);
                countBadgeToggle.SetValueWithoutNotify(ToolPreferences.ShowAssetCountBadge);
                editScriptToggle.SetValueWithoutNotify(ToolPreferences.ShowEditScriptButton);
                showAssetsToggle.SetValueWithoutNotify(ToolPreferences.ShowShowAssetsButton);
            };

            return section;
        }

        static VisualElement BuildAssetCreationSection(out Action refresh)
        {
            var section = new VisualElement();
            section.Add(SectionHeader("Asset Creation"));

            var helpBox = new HelpBox(string.Empty, HelpBoxMessageType.None);
            var modeField = new EnumField("New asset folder", ToolPreferences.CreationFolderMode);

            var folderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
            var folderField = new TextField("Folder") { value = ToolPreferences.FixedDefaultFolder };
            folderField.style.flexGrow = 1;
            var browseButton = new Button { text = "Browse..." };
            browseButton.style.width = 70;
            folderRow.Add(folderField);
            folderRow.Add(browseButton);

            void UpdateVisibility(AssetCreationFolderMode mode)
            {
                helpBox.text = GetModeDescription(mode);
                folderRow.style.display = mode == AssetCreationFolderMode.FixedDefaultFolder ? DisplayStyle.Flex : DisplayStyle.None;
            }

            modeField.RegisterValueChangedCallback(evt =>
            {
                var mode = (AssetCreationFolderMode)evt.newValue;
                ToolPreferences.CreationFolderMode = mode;
                UpdateVisibility(mode);
            });

            folderField.RegisterValueChangedCallback(evt => ToolPreferences.FixedDefaultFolder = evt.newValue);

            browseButton.clicked += () =>
            {
                string chosen = EditorUtility.OpenFolderPanel("Choose Default Folder", "Assets", string.Empty);
                if (string.IsNullOrEmpty(chosen))
                    return;

                string relative = MakeProjectRelative(chosen);
                folderField.value = relative;
                ToolPreferences.FixedDefaultFolder = relative;
            };

            UpdateVisibility(ToolPreferences.CreationFolderMode);

            section.Add(modeField);
            section.Add(helpBox);
            section.Add(folderRow);

            refresh = () =>
            {
                modeField.SetValueWithoutNotify(ToolPreferences.CreationFolderMode);
                folderField.SetValueWithoutNotify(ToolPreferences.FixedDefaultFolder);
                UpdateVisibility(ToolPreferences.CreationFolderMode);
            };

            return section;
        }

        static string GetModeDescription(AssetCreationFolderMode mode)
        {
            switch (mode)
            {
                case AssetCreationFolderMode.MostCommonFolderForType:
                    return "New assets are placed in whichever folder already contains the most assets of " +
                        "that type, falling back to the active Project window folder if none exist yet.";
                case AssetCreationFolderMode.FixedDefaultFolder:
                    return "New assets (of any type) are always placed in this folder.";
                case AssetCreationFolderMode.PromptEachTime:
                    return "You'll be asked to choose a folder every time you create an asset.";
                default:
                    return "New assets are placed in whichever folder is currently open/selected in the Project window.";
            }
        }

        static string MakeProjectRelative(string absolutePath)
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath)?.Replace('\\', '/');
            absolutePath = absolutePath.Replace('\\', '/');

            if (projectRoot != null && absolutePath.StartsWith(projectRoot))
                return absolutePath.Substring(projectRoot.Length + 1);

            return "Assets";
        }

        static VisualElement BuildAssembliesSection(out Action refresh)
        {
            var section = new VisualElement { style = { marginTop = 16 } };
            section.Add(SectionHeader("Included Assemblies"));
            section.Add(new HelpBox(
                "Assemblies compiled from Assets/ are always scanned for ScriptableObject types. " +
                "Enable any other assembly below (Unity packages, plugins, etc.) to include ALL its types.",
                HelpBoxMessageType.Info));

            var searchField = new ToolbarSearchField { style = { marginTop = 4, marginBottom = 4 } };
            var scrollView = new ScrollView { style = { minHeight = 160, maxHeight = 260 } };
            section.Add(searchField);
            section.Add(scrollView);

            var rows = new List<(VisualElement element, string searchText)>();

            void Rebuild()
            {
                scrollView.Clear();
                rows.Clear();

                var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor).OrderBy(a => a.name).ToList();

                bool alternate = false;
                foreach (var assembly in assemblies)
                {
                    bool isAssetsAssembly = AssemblyFilterSettings.IsAssetsAssembly(assembly);
                    bool included = isAssetsAssembly || AssemblyFilterSettings.IsExtraIncluded(assembly.name);

                    var row = BuildToggleRow(assembly.name, included, isAssetsAssembly, newValue =>
                    {
                        if (!isAssetsAssembly)
                            AssemblyFilterSettings.SetExtraIncluded(assembly.name, newValue);
                    }, alternate);

                    scrollView.Add(row);
                    rows.Add((row, assembly.name));
                    alternate = !alternate;
                }

                ApplySearch(searchField.value, rows);
            }

            searchField.RegisterValueChangedCallback(evt => ApplySearch(evt.newValue, rows));

            Rebuild();
            refresh = Rebuild;
            return section;
        }

        static VisualElement BuildClassesSection(out Action refresh)
        {
            var section = new VisualElement { style = { marginTop = 16 } };
            section.Add(SectionHeader("Included Individual Classes"));
            section.Add(new HelpBox(
                "Pick specific ScriptableObject classes to include even when their assembly isn't enabled above.",
                HelpBoxMessageType.Info));

            var searchField = new ToolbarSearchField { style = { marginTop = 4, marginBottom = 4 } };
            var scrollView = new ScrollView { style = { minHeight = 160, maxHeight = 260 } };
            section.Add(searchField);
            section.Add(scrollView);

            var rows = new List<(VisualElement element, string searchText)>();

            void Rebuild()
            {
                scrollView.Clear();
                rows.Clear();

                var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor).ToList();
                var isAssetsAssemblyByName = assemblies.ToDictionary(a => a.name, AssemblyFilterSettings.IsAssetsAssembly);
                var types = TypeDiscovery.GetAllEligibleTypes().OrderBy(t => t.FullName).ToList();

                bool alternate = false;
                foreach (var type in types)
                {
                    string assemblyName = type.Assembly.GetName().Name;
                    bool assemblyIncluded = (isAssetsAssemblyByName.TryGetValue(assemblyName, out bool isAssets) && isAssets)
                        || AssemblyFilterSettings.IsExtraIncluded(assemblyName);
                    bool included = assemblyIncluded || AssemblyFilterSettings.IsTypeExplicitlyIncluded(type);

                    var row = BuildToggleRow(type.FullName, included, assemblyIncluded, newValue =>
                    {
                        if (!assemblyIncluded)
                            AssemblyFilterSettings.SetTypeExplicitlyIncluded(type, newValue);
                    }, alternate);

                    scrollView.Add(row);
                    rows.Add((row, type.FullName));
                    alternate = !alternate;
                }

                ApplySearch(searchField.value, rows);
            }

            searchField.RegisterValueChangedCallback(evt => ApplySearch(evt.newValue, rows));

            Rebuild();
            refresh = Rebuild;
            return section;
        }

        static VisualElement BuildHiddenTypesSection(out Action refresh)
        {
            var section = new VisualElement { style = { marginTop = 16 } };
            section.Add(SectionHeader("Hidden Types"));
            section.Add(new HelpBox(
                "Types hidden from the tool window via its row ⋮ options menu. Untick to bring one back.",
                HelpBoxMessageType.Info));

            var emptyLabel = new Label("Nothing is hidden.");
            emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            emptyLabel.style.opacity = 0.6f;

            var searchField = new ToolbarSearchField { style = { marginTop = 4, marginBottom = 4 } };
            var scrollView = new ScrollView { style = { minHeight = 80, maxHeight = 200 } };
            section.Add(emptyLabel);
            section.Add(searchField);
            section.Add(scrollView);

            var rows = new List<(VisualElement element, string searchText)>();

            void Rebuild()
            {
                scrollView.Clear();
                rows.Clear();

                var keys = HiddenTypesSettings.GetAllHiddenKeys().ToList();
                bool any = keys.Count > 0;
                emptyLabel.style.display = any ? DisplayStyle.None : DisplayStyle.Flex;
                searchField.style.display = any ? DisplayStyle.Flex : DisplayStyle.None;
                scrollView.style.display = any ? DisplayStyle.Flex : DisplayStyle.None;

                bool alternate = false;
                foreach (var key in keys)
                {
                    var type = Type.GetType(key);
                    string displayName = type != null ? type.FullName : $"{key} (type not found)";

                    var row = BuildToggleRow(displayName, true, false, newValue =>
                    {
                        if (newValue)
                            return;

                        if (type != null)
                            HiddenTypesSettings.SetHidden(type, false);
                        else
                            HiddenTypesSettings.SetHiddenByKey(key, false);

                        Rebuild();
                    }, alternate);

                    scrollView.Add(row);
                    rows.Add((row, displayName));
                    alternate = !alternate;
                }

                ApplySearch(searchField.value, rows);
            }

            searchField.RegisterValueChangedCallback(evt => ApplySearch(evt.newValue, rows));

            Rebuild();
            refresh = Rebuild;
            return section;
        }

        static Label SectionHeader(string text)
        {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4;
            return label;
        }

        static VisualElement BuildToggleRow(string label, bool value, bool disabled, Action<bool> onChanged, bool alternate)
        {
            var row = new VisualElement
            {
                style =
                {
                    paddingLeft = 4, paddingRight = 4, paddingTop = 2, paddingBottom = 2,
                    backgroundColor = alternate ? new Color(1f, 1f, 1f, 0.035f) : new Color(0, 0, 0, 0)
                }
            };

            var toggle = new Toggle(label) { value = value };
            toggle.SetEnabled(!disabled);
            toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            row.Add(toggle);

            return row;
        }

        static void ApplySearch(string query, List<(VisualElement element, string searchText)> rows)
        {
            bool hasQuery = !string.IsNullOrWhiteSpace(query);
            foreach (var (element, text) in rows)
            {
                bool isMatch = !hasQuery || text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                element.style.display = isMatch ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
