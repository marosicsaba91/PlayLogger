#if UNITY_EDITOR
using System;  
using MUtility;
using UnityEditor; 
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayLogging
{

public sealed class LogSourceColumn : Column<Log>
{
    public LogSourceColumn(ColumnInfo info = null) : base(info) { }
    
    public override void DrawCell(Rect position, Log row, GUIStyle style, Action onChanged)
    {
        if(row == null) return;
        string sourceName = row.SourceName;
        if (string.IsNullOrEmpty(sourceName)) return;
        
        if (row.Source != null)
        {
            var color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            EditorGUI.DrawRect(position, color);
        }

        var content = new GUIContent( sourceName, null, sourceName);
        if (!GUI.Button(position, content, style)) return;

        if (row.Source != null)
            PingObject(row.Source);
    }
    
    static void PingObject(Object objectToPing)
    {
        if (objectToPing == null) return;
        EditorGUIUtility.PingObject(objectToPing);
    }

    protected override GUIStyle GetDefaultStyle() => new GUIStyle(GUI.skin.label)
    {
        padding = new RectOffset(2, 2, 0, 0)
    };
}
}
#endif
