using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using CompilationAssembly = UnityEditor.Compilation.Assembly;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class AssemblyFilterSettings
    {
        const string ExtraIncludedAssembliesKey = "Chromum.ScriptableObjectUtility.ExtraIncludedAssemblies";
        const string ExtraIncludedTypesKey = "Chromum.ScriptableObjectUtility.ExtraIncludedTypes";

        public static event Action IncludedAssembliesChanged;

        static HashSet<string> extraIncludedAssemblies;
        static HashSet<string> extraIncludedTypes;

        static HashSet<string> ExtraIncludedAssemblies
        {
            get
            {
                if (extraIncludedAssemblies == null)
                    extraIncludedAssemblies = LoadSet(ExtraIncludedAssembliesKey);
                return extraIncludedAssemblies;
            }
        }

        static HashSet<string> ExtraIncludedTypes
        {
            get
            {
                if (extraIncludedTypes == null)
                    extraIncludedTypes = LoadSet(ExtraIncludedTypesKey);
                return extraIncludedTypes;
            }
        }

        public static bool IsAssetsAssembly(CompilationAssembly assembly)
        {
            return assembly.sourceFiles.Any(f => f.Replace('\\', '/').StartsWith("Assets/", StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsIncluded(CompilationAssembly assembly)
        {
            return IsAssetsAssembly(assembly) || IsExtraIncluded(assembly.name);
        }

        public static bool IsExtraIncluded(string assemblyName)
        {
            return ExtraIncludedAssemblies.Contains(assemblyName);
        }

        public static void SetExtraIncluded(string assemblyName, bool included)
        {
            bool changed = included ? ExtraIncludedAssemblies.Add(assemblyName) : ExtraIncludedAssemblies.Remove(assemblyName);

            if (!changed)
                return;

            EditorPrefs.SetString(ExtraIncludedAssembliesKey, string.Join(";", ExtraIncludedAssemblies));
            IncludedAssembliesChanged?.Invoke();
        }

        public static bool IsTypeExplicitlyIncluded(Type type)
        {
            return ExtraIncludedTypes.Contains(type.AssemblyQualifiedName);
        }

        public static void SetTypeExplicitlyIncluded(Type type, bool included)
        {
            string key = type.AssemblyQualifiedName;
            bool changed = included ? ExtraIncludedTypes.Add(key) : ExtraIncludedTypes.Remove(key);

            if (!changed)
                return;

            EditorPrefs.SetString(ExtraIncludedTypesKey, string.Join(";", ExtraIncludedTypes));
            IncludedAssembliesChanged?.Invoke();
        }

        static HashSet<string> LoadSet(string key)
        {
            string raw = EditorPrefs.GetString(key, string.Empty);
            return new HashSet<string>(raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static void ResetAll()
        {
            EditorPrefs.DeleteKey(ExtraIncludedAssembliesKey);
            EditorPrefs.DeleteKey(ExtraIncludedTypesKey);
            extraIncludedAssemblies = null;
            extraIncludedTypes = null;
            IncludedAssembliesChanged?.Invoke();
        }
    }
}
