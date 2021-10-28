#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MUtility;
using Object = UnityEngine.Object;

namespace PlayLogging
{
public class EditorWindowHeader
{
    public const int headerHeight = 20;

    static readonly Color lightHeaderColor = new Color(0.24f, 0.24f, 0.24f);
    static readonly Color lightHeaderColorHover = new Color(0.27f, 0.27f, 0.27f);
    static readonly Color lightHeaderColorEnabled = new Color(0.31f, 0.31f, 0.31f);

    static readonly Color darkHeaderColor = new Color(0.24f, 0.24f, 0.24f);
    static readonly Color darkHeaderColorHover = new Color(0.27f, 0.27f, 0.27f);
    static readonly Color darkHeaderColorEnabled = new Color(0.31f, 0.31f, 0.31f);
    public static int HeaderHeight => headerHeight;
    public Rect FullHeaderPosition { get; private set; }

    static Color GetColor(bool hover = false, bool enabled = false)
    {
        if (EditorGUIUtility.isProSkin)
        {
            if (enabled) return darkHeaderColorEnabled;
            return hover ? darkHeaderColorHover : darkHeaderColor;
        }

        if (enabled) return lightHeaderColorEnabled;
        return hover ? lightHeaderColorHover : lightHeaderColor;
    }

    float _headerX;
    Rect _windowPosition;

    static GUIStyle _buttonStyle;
    static GUIStyle _centeredButtonStyle;

    public Rect SetupHeader(Rect windowPosition)
    {
        InitStatic();
        _windowPosition = windowPosition;
        FullHeaderPosition = new Rect(0, 0, windowPosition.width, headerHeight);
        EditorHelper.DrawBox(FullHeaderPosition, false);
        _headerX = 0;
        return FullHeaderPosition;
    }

    static void InitStatic()
    {
        _buttonStyle = new GUIStyle(GUI.skin.label);
        _centeredButtonStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };
    }


    public void Space(float widthInPixel) => _headerX += widthInPixel;


    public bool DrawButton(int width, string label)
    {
        var rect = new Rect(_headerX, 0, width, headerHeight);
        _headerX += width;
        return GUI.Button(rect, label, EditorStyles.toolbarButton);
    }


    public T DrawEnumDropdown<T>(
        int width,
        T value) where T : Enum
    {
        var rect = new Rect(_headerX, 0, width, headerHeight);
        _headerX += width;
        return (T) EditorGUI.EnumPopup(rect, value, EditorStyles.toolbarPopup);
    }


    public bool DrawIconButton(string iconID)
    {
        GUIContent guiContent = EditorGUIUtility.IconContent(iconID);

        var rect = new Rect(_headerX, 0, headerHeight, headerHeight);

        _headerX += headerHeight;
        bool result = GUI.Button(rect, guiContent, EditorStyles.toolbarButton);
        GUI.Label(rect, guiContent, _centeredButtonStyle);
        return result;
    }

    public bool DrawToggleButton(int width, GUIContent content, bool isChecked)
    {
        var rect = new Rect(_headerX, 0, width, headerHeight);
        _headerX += width;
        Color original = GUI.color;
        if (!isChecked)
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
        bool change = GUI.Button(rect, content, EditorStyles.toolbarButton);
        GUI.color = original;
        return change ? !isChecked : isChecked;
    }

    public class PopupElement
    {
        public readonly GUIContent content;
        public readonly Func<bool> valueGetter;
        public readonly Action<bool> valueSetter;

        public PopupElement(GUIContent content, Func<bool> valueGetter, Action<bool> valueSetter)
        {
            this.valueGetter = valueGetter;
            this.valueSetter = valueSetter;
            this.content = content;
        }

        public PopupElement(string content, Func<bool> valueGetter, Action<bool> valueSetter) :
            this(new GUIContent(content), valueGetter, valueSetter)
        {
        }
    }

    public void DrawMultiSelectPopupButton(int width, IEnumerable<PopupElement> elements, string title = null)
    {
        var position = new Rect(_headerX, 0, width, headerHeight);
        _headerX += width;

        GUIContent titleContent = string.IsNullOrEmpty(title)
            ? EditorGUIUtility.IconContent("Icon Dropdown")
            : new GUIContent(title);

        if (!GUI.Button(position, titleContent, EditorStyles.toolbarButton)) return;

        var menu = new GenericMenu();
        foreach (PopupElement t in elements)
            menu.AddItem(new GUIContent(t.content), t.valueGetter(), Change, t);

        menu.ShowAsContext();

        void Change(object element) =>
            ((PopupElement) element).valueSetter(!((PopupElement) element).valueGetter());
    }

    public T DrawObjectField<T>(
        int width,
        T obj,
        Type type,
        Action<T, T> onChangeCallback = null) where T : Object
    {
        var position = new Rect(_headerX, 0, width, headerHeight);
        _headerX += width;

        position.y += 1;
        position.height -= 2;
        position.x += 1;
        position.width -= 2;
        var newValue = (T) EditorGUI.ObjectField(position, obj, type, false);
        if (onChangeCallback != null && newValue != obj)
            onChangeCallback(obj, newValue);
        return newValue;
    }
}
}
#endif