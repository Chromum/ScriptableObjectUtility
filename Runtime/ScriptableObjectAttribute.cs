using System;

namespace Bonejam.ScriptableObjectUtility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScriptableObjectAttribute : Attribute
    {
        readonly string path;

        public ScriptableObjectAttribute(string path)
        {
            this.path = path;
        }

        public string scriptableObjectPath => path;
    }
}
