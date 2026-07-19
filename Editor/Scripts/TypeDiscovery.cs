using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bonejam.ScriptableObjectUtility.Editor
{
    internal static class TypeDiscovery
    {
        public const string GlobalGroupName = "Global";
        static readonly char[] PathDelimiters = { '/', '\\' };

        public static IEnumerable<Type> DiscoverScriptableObjectTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(t => !t.IsAbstract
                    && !t.IsGenericTypeDefinition
                    && !typeof(UnityEditor.Editor).IsAssignableFrom(t)
                    && !typeof(EditorWindow).IsAssignableFrom(t));
        }

        public static string[] ResolveGroupPath(Type type, out string displayName)
        {
            var attribute = type.GetCustomAttribute<ScriptableObjectAttribute>();

            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.scriptableObjectPath))
            {
                string[] segments = attribute.scriptableObjectPath.Split(PathDelimiters, StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length == 1)
                {
                    // A single segment has nothing left over to act as a folder + a label,
                    // so treat it as the group name and keep the default type-name label.
                    displayName = type.Name;
                    return segments;
                }

                if (segments.Length > 1)
                {
                    displayName = segments[segments.Length - 1];
                    return segments.Take(segments.Length - 1).ToArray();
                }
            }

            displayName = type.Name;
            string ns = type.Namespace;
            return string.IsNullOrEmpty(ns) ? new[] { GlobalGroupName } : ns.Split('.');
        }

        public static void ValidateMisusedAttributes(IEnumerable<Type> discoveredTypes)
        {
            var discoveredSet = new HashSet<Type>(discoveredTypes);

            foreach (var type in TypeCache.GetTypesWithAttribute<ScriptableObjectAttribute>())
            {
                if (!discoveredSet.Contains(type))
                {
                    Debug.LogWarning($"{type.FullName} uses [ScriptableObjectAttribute] but isn't a creatable ScriptableObject type " +
                        "(it may be abstract, a generic definition, an Editor/EditorWindow type, or not derived from ScriptableObject at all). The attribute will be ignored.");
                }
            }
        }
    }
}
