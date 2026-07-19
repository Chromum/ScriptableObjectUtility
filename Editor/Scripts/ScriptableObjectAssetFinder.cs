using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class ScriptableObjectAssetFinder
    {
        public static List<ScriptableObject> FindAssetsOfExactType(Type type)
        {
            var results = new List<ScriptableObject>();
            var guids = AssetDatabase.FindAssets($"t:{type.Name}");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset != null && asset.GetType() == type)
                    results.Add(asset);
            }

            return results;
        }

        public static MonoScript FindScriptForType(Type type)
        {
            return Resources.FindObjectsOfTypeAll<MonoScript>()
                .FirstOrDefault(s => s.GetClass() == type);
        }
    }
}
