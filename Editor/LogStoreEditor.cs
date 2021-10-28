#if UNITY_EDITOR
using UnityEngine;
 
using UnityEditor; 

namespace PlayLogging
{
[CustomEditor(typeof(LogStore))]
public class LogStoreEditor : Editor
{
    const string resourceFilename = "custom-editor-uie";

    public override void OnInspectorGUI()
    {
        var logSet = (LogStore) target;
        if (GUILayout.Button($"Clear {logSet.Logs.Count} Logs"))
            logSet.ClearLogs();

        if (logSet.IsRestorable)
            if (GUILayout.Button($"Restore Last Cleared"))
                logSet.Restore();
 
        logSet.ClearOnPlay = GUILayout.Toggle(logSet.ClearOnPlay, "Clear on Play"); 
        logSet.ClearOnBuild = GUILayout.Toggle(logSet.ClearOnBuild, "Clear on Build"); 
        logSet.IncludeUnityLogsInEditor = GUILayout.Toggle(logSet.IncludeUnityLogsInEditor, "Include Unity logs in Editor"); 
    }
}
}
# endif