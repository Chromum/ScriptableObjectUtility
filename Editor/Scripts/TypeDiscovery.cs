using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class TypeDiscovery
    {
        public const string GlobalGroupName = "Global";
        static readonly char[] PathDelimiters = { '/', '\\' };

        static readonly HashSet<string> DenylistedBaseTypeFullNames = new HashSet<string>
        {
            "UnityEngine.Rendering.Universal.ScriptableRendererFeature",
            "UnityEngine.Rendering.VolumeComponent",
            "UnityEditor.AssetImporters.ScriptedImporter",
            "UnityEditor.Experimental.AssetImporters.ScriptedImporter",
            "UnityEditor.AssetImporter",
        };

        public static IEnumerable<Type> DiscoverScriptableObjectTypes()
        {
            var includedAssemblyNames = new HashSet<string>(
                CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                    .Where(AssemblyFilterSettings.IsIncluded)
                    .Select(a => a.name));

            return GetAllEligibleTypes()
                .Where(t => (includedAssemblyNames.Contains(t.Assembly.GetName().Name)
                        || AssemblyFilterSettings.IsTypeExplicitlyIncluded(t))
                    && !HiddenTypesSettings.IsHidden(t));
        }

        public static IEnumerable<Type> GetAllEligibleTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(t => !t.IsAbstract
                    && !t.IsGenericTypeDefinition
                    && !typeof(UnityEditor.Editor).IsAssignableFrom(t)
                    && !typeof(EditorWindow).IsAssignableFrom(t)
                    && !InheritsFromDenylistedBase(t));
        }

        static bool InheritsFromDenylistedBase(Type type)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if (DenylistedBaseTypeFullNames.Contains(baseType.FullName))
                    return true;
            }

            return false;
        }

        public static string[] ResolveGroupPath(Type type, out string displayName)
        {
            var attribute = type.GetCustomAttribute<ScriptableObjectAttribute>();

            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.scriptableObjectPath))
            {
                string[] segments = attribute.scriptableObjectPath.Split(PathDelimiters, StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length == 1)
                {
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
