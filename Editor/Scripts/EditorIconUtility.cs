using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chromum.ScriptableObjectUtility.Editor
{
    internal static class EditorIconUtility
    {
        public static void SetIcon(Button button, string iconName, string tooltip)
        {
            if (button == null)
                return;

            button.tooltip = tooltip;

            var content = EditorGUIUtility.IconContent(iconName);
            if (content?.image != null)
                button.style.backgroundImage = new StyleBackground((Texture2D)content.image);
        }
    }
}
