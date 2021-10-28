using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayLogging
{
    class GUIContentIcon
    {
        public GUIContentIcon(string icon, string tooltip = null)
        {
#if UNITY_EDITOR
            GuiContent = EditorGUIUtility.IconContent(icon);
            if(tooltip!=null)
                GuiContent.tooltip = tooltip;
#endif
        }
         
        GUIContent GuiContent { get; } = null;

        public static implicit operator GUIContent(GUIContentIcon d) => d.GuiContent;
    }
}