#if UNITY_EDITOR
using UnityEngine;

namespace PlayLogging
{
public static class LogTypeEditorHelper
{
    internal static readonly GUIContentIcon error = new GUIContentIcon("console.erroricon.sml", "Unity Error");
    internal static readonly GUIContentIcon assert = new GUIContentIcon("d_winbtn_mac_close", "Unity Assert");
    internal static readonly GUIContentIcon warning = new GUIContentIcon("console.warnicon.sml", "Unity Warning");
    internal static readonly GUIContentIcon log = new GUIContentIcon("console.infoicon.sml", "Unity Log");
    internal static readonly GUIContentIcon exception = new GUIContentIcon("CollabError", "Unity Exception");
    internal static readonly GUIContentIcon custom = new GUIContentIcon("DotFill", "Custom Log");


    public static GUIContent ToIcon(LogType logType)
    {
        switch (logType)
        {
            case LogType.UnityError:
                return error;
            case LogType.UnityAssert:
                return assert;
            case LogType.UnityWarning:
                return warning;
            case LogType.UnityLog:
                return log;
            case LogType.Exception:
                return exception;
            case LogType.PlayLog:
                return custom;
            default:
                return GUIContent.none;
        }
    }
}
}
#endif