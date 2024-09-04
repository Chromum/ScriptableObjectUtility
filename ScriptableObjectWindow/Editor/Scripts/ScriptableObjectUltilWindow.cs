#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class ScriptableObjectUltilWindow : EditorWindow, IHasCustomMenu
{
    public static List<Type> types = new List<Type>();
    public static Dictionary<string, PathFoldout> foldoutGroups = new Dictionary<string, PathFoldout>();
    public static VisualElement foldoutRootElements;


    public static VisualTreeAsset _base, _foldout, _button;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/Scriptable Object Utlity")]
    public static void ShowExample()
    {
        ScriptableObjectUltilWindow wnd = GetWindow<ScriptableObjectUltilWindow>();
        wnd.titleContent = new GUIContent("ScriptableObjectUltilWindow");
    }

    private static void PopulateMenu()
    {
        var attType = typeof(ScriptableObjectAttribute);
        var collectedList = TypeCache.GetTypesWithAttribute<ScriptableObjectAttribute>();

        foreach (var type in collectedList)
        {
            if (types.Contains(type))
                continue;

            if (type.BaseType != typeof(ScriptableObject))
            {
                Debug.LogError($"{type.FullName} uses the Attribute ScriptableObjectAttribute but does not derrive from ScriptableObject.");
                continue;
            }


            types.Add(type);
        }



        foreach (var item in types)
        {
            var att = item.GetCustomAttribute<ScriptableObjectAttribute>();
            char[] delimiters = new char[] {'/',  '\\'};
            string[] result = att.scriptableObjectPath.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (result.Length <= 0)
            {
                Debug.LogError($"{item.FullName} does not contain a valid path. A valid path requires one or more folder items as well as an object name.");
                continue;
            }

            string finalName = result.Last();

            /*
             * Test:
             * 0     1     2
             * Hello/World
            */

            PathFoldout currentFoldout = null;

            for (int i = 0; i < result.Length; i++)
            {
                if (i == 0)
                {
                    if (foldoutGroups.ContainsKey(result[i]))
                        currentFoldout = foldoutGroups[result[i]];
                    else
                    {
                        foldoutGroups.Add(result[i], new PathFoldout(foldoutRootElements, result[i], _button, _foldout));
                        currentFoldout = foldoutGroups[result[i]];
                    }
                        
                }
                else if (i != result.Length - 1)
                {
                    if (currentFoldout.subDirectorys.ContainsKey(result[i]))
                        currentFoldout = currentFoldout.subDirectorys[result[i]];
                    else
                    {
                        currentFoldout.subDirectorys.Add(result[i], new PathFoldout(currentFoldout.pathFoldout, result[i],_button, _foldout));
                        currentFoldout = currentFoldout.subDirectorys[result[i]];
                    }
                }
                else
                {
                    var methodInfo = typeof(PathFoldout).GetMethod("AddButton");
                    var genericMethod = methodInfo.MakeGenericMethod(item);

                    genericMethod.Invoke(currentFoldout, new object[] { result[i] });
                }
            }
        }
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        GUIContent content = new GUIContent("Refresh ScriptableObject List");
        menu.AddItem(content, false, PopulateMenu);
    }

    public void CreateGUI()
    {
        #region Setup
        _base = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScriptableObjectWindow/Editor/UXML/ScriptableObjectUltilWindow.uxml");
        _button = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScriptableObjectWindow/Editor/UXML/SOButton.uxml");
        _foldout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScriptableObjectWindow/Editor/UXML/foldouts-root.uxml");
        #endregion


        foldoutGroups.Clear();
        VisualElement root = rootVisualElement;
        VisualElement tree = _base.Instantiate();

        root.Add(tree);

        foldoutRootElements = root.Q<Foldout>("foldouts-root");


        PopulateMenu();
    }
}


public class PathFoldout
{
    public string pathOrigin;
    public Dictionary<string, PathFoldout> subDirectorys = new Dictionary<string, PathFoldout>();
    Dictionary<string, Button> buttons = new Dictionary<string, Button>();
    VisualTreeAsset bttnAsset;

    public PathFoldout(VisualElement baseElement, string name, VisualTreeAsset bttn, VisualTreeAsset foldout)
        {
        var foldoutAsset = foldout;

        var e = foldoutAsset.Instantiate();
        pathFoldout = e.Q<Foldout>("foldouts-root");
        pathFoldout.text = name;
        baseElement.Add(pathFoldout);


        bttnAsset = bttn;
    }

    public void AddButton<T>(string path) where T : ScriptableObject
    {
        if (buttons.ContainsKey(path))
            return;
        
        var e = bttnAsset.Instantiate();
        var button = e.Q<Button>("namedBttn");
        button.text = path;

        button.clicked += () =>
        {
            var createdSO = ScriptableObject.CreateInstance<T>();
            createdSO.name = $"New {typeof(T).Name}";

            #region Get Current Project Path (https://discussions.unity.com/t/how-to-get-path-from-the-current-opened-folder-in-the-project-window-in-unity-editor/226209)
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            string pathToCurrentFolder = obj.ToString();
            #endregion

            ProjectWindowUtil.CreateAsset(createdSO, $"{pathToCurrentFolder}/{createdSO.name}.asset");
        };


        pathFoldout.Add(button);

        ReorderFoldoutChildren(pathFoldout);
        buttons.Add(path, button);
    }

    public void ReorderFoldoutChildren(Foldout parentFoldout)
    {
        // Create a list to hold the foldout children
        List<VisualElement> foldoutChildren = new List<VisualElement>();
        // Create a list to hold the non-foldout children
        List<VisualElement> otherChildren = new List<VisualElement>();

        // Loop through the children of the parent foldout
        foreach (var child in parentFoldout.Children())
        {
            if (child is Foldout)
            {
                foldoutChildren.Add(child);
            }
            else
            {
                otherChildren.Add(child);
            }
        }

        // Clear the parent foldout's children
        parentFoldout.Clear();

        // Add the foldout children first
        foreach (var foldoutChild in foldoutChildren)
        {
            parentFoldout.Add(foldoutChild);
        }

        // Add the other children next
        foreach (var otherChild in otherChildren)
        {
            parentFoldout.Add(otherChild);
        }
    }

    public Foldout pathFoldout;
}
#endif