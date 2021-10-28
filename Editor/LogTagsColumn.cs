#if UNITY_EDITOR
using System; 
using MUtility;
using UnityEditor; 
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayLogging
{

public sealed class LogTagsColumn : Column<Log>
{
    public LogTagsColumn(
        ColumnInfo info = null) :
        base( info)
    {
    }

    public override void DrawCell(Rect position, Log row, GUIStyle style, Action onChanged)
    {
        if (row?.Tags == null || row.Tags.Length <= 0) return;
        float x = position.x;
        float w = position.width / row.Tags.Length;
        foreach (LogTag tag in row.Tags)
        {
            position = new Rect(x, position.y, w, position.height);
            x = position.xMax;
            Color color;
            if (tag == null)
            {
                color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
                EditorGUI.DrawRect(position, color);
                GUI.Label(position, "null", style);
            }
            else 
            {
                color = tag.color;
                EditorGUI.DrawRect(position, color);
                GUIStyle buttonStyle = color.r + color.g + color.b < 1 ? WhiteLabelStyle : BlackLabelStyle;
                    
                var content = new GUIContent( tag.name, null, tag.name);
                if (GUI.Button(position, content, buttonStyle))
                    PingObject(tag);
            } 
        }
    }

    protected override GUIStyle GetDefaultStyle() => CenteredLabelStyle;

    static void PingObject(Object objectToPing)
    {
        if (objectToPing == null) return;
        EditorGUIUtility.PingObject(objectToPing);
    }
    
    GUIStyle _whiteLabelStyle;

    GUIStyle WhiteLabelStyle
    {
        get
        {
            if (_whiteLabelStyle != null) return _whiteLabelStyle;
            _whiteLabelStyle = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(2, 2, 0, 0),
                normal = {textColor = Color.white},
                focused = {textColor = Color.white},
                active = {textColor = Color.white},
                hover = {textColor = Color.white},
            };

            return _whiteLabelStyle;
        }
    }

    GUIStyle _blackLabelStyle;

    GUIStyle BlackLabelStyle
    {
        get
        {
            if (_blackLabelStyle != null) return _blackLabelStyle;
            _blackLabelStyle = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(2, 2, 0, 0),
                normal = {textColor = Color.black},
                focused = {textColor = Color.black},
                active = {textColor = Color.black},
                hover = {textColor = Color.black},

            };

            return _blackLabelStyle;
        }
    }
    
    GUIStyle _centeredLabelStyle;

    GUIStyle CenteredLabelStyle
    {
        get
        {
            if (_centeredLabelStyle != null) return _centeredLabelStyle;
            _centeredLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            };
            return _centeredLabelStyle;
        }
    }
}
}
    
#endif