using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bonejam.ScriptableObjectUtility.Editor
{
    internal class PathFoldoutTree
    {
        static readonly MethodInfo AddButtonMethod = typeof(PathFoldoutTree).GetMethod(nameof(AddButton));
        static readonly MethodInfo GetActiveFolderPathMethod =
            typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        public readonly string fullPath;
        public readonly Foldout foldout;
        public readonly Dictionary<string, PathFoldoutTree> subDirectories = new Dictionary<string, PathFoldoutTree>();

        readonly VisualTreeAsset buttonAsset;
        readonly Dictionary<string, VisualElement> buttonRows = new Dictionary<string, VisualElement>();
        readonly Dictionary<string, string> buttonTypeFullNames = new Dictionary<string, string>();

        public PathFoldoutTree(VisualElement parent, string name, string fullPath, VisualTreeAsset buttonAsset, VisualTreeAsset foldoutAsset)
        {
            this.fullPath = fullPath;
            this.buttonAsset = buttonAsset;

            var instance = foldoutAsset.Instantiate();
            foldout = instance.Q<Foldout>("foldout-root");
            foldout.text = name;
            foldout.value = FoldoutExpandStateStore.GetExpanded(fullPath);
            foldout.RegisterValueChangedCallback(evt => FoldoutExpandStateStore.SetExpanded(fullPath, evt.newValue));

            parent.Add(foldout);
        }

        public void AddButton<T>(string label) where T : ScriptableObject
        {
            if (buttonRows.ContainsKey(label))
                return;

            var instance = buttonAsset.Instantiate();
            var button = instance.Q<Button>("namedBttn");
            var icon = instance.Q<Image>("typeIcon");

            button.text = label;
            button.tooltip = typeof(T).FullName;
            if (icon != null)
                icon.image = EditorGUIUtility.ObjectContent(null, typeof(T)).image;

            button.clicked += () => CreateAsset<T>();

            foldout.Add(instance);
            ReorderFoldoutChildren(foldout);

            buttonRows.Add(label, instance);
            buttonTypeFullNames.Add(label, typeof(T).FullName);
        }

        public static void AddButtonReflective(PathFoldoutTree tree, Type type, string label)
        {
            var genericMethod = AddButtonMethod.MakeGenericMethod(type);
            genericMethod.Invoke(tree, new object[] { label });
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
            var createdSO = ScriptableObject.CreateInstance<T>();
            createdSO.name = $"New {typeof(T).Name}";

            string activeFolder = (string)GetActiveFolderPathMethod.Invoke(null, null);
            ProjectWindowUtil.CreateAsset(createdSO, $"{activeFolder}/{createdSO.name}.asset");
            Undo.RegisterCreatedObjectUndo(createdSO, $"Create {typeof(T).Name}");
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
