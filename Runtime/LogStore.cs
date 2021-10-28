using System;
using System.Collections.Generic;
using MUtility;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
#endif

namespace PlayLogging
{
[CreateAssetMenu(fileName = "Log Store", menuName = "Play Logging/Log Store")]
public class LogStore : ScriptableObject
#if UNITY_EDITOR
    , IPreprocessBuildWithReport
#endif
{
    [SerializeField] List<Log> logs = new List<Log>();
    [SerializeField] bool clearOnPlay = true;
    [SerializeField] bool clearOnBuild = true;
    [FormerlySerializedAs("includeUnityLogsInEditor")] [SerializeField] bool includeUnityLogsInEditorInEditor = false;

    public bool ClearOnPlay
    {
        get => clearOnPlay;
        set
        {
            if (clearOnPlay == value) return;
            clearOnPlay = value;
            SetObjectDirty();
        }
    }

    public bool ClearOnBuild
    {
        get => clearOnBuild;
        set
        {
            if (clearOnBuild == value) return;
            clearOnBuild = value;
            SetObjectDirty();
        }
    }
    
    public bool IncludeUnityLogsInEditor
    {
        get => includeUnityLogsInEditorInEditor;
        set
        {
            if (includeUnityLogsInEditorInEditor == value) return;
            includeUnityLogsInEditorInEditor = value;
            if(includeUnityLogsInEditorInEditor)
                Application.logMessageReceivedThreaded += UnityLog;
            else
                Application.logMessageReceivedThreaded -= UnityLog;
            SetObjectDirty();
        }
    }


    [SerializeField] bool clearObBuild;

    public event Action<Log> LogReceived;
    public event Action LogsChanged;
    public event Action LogsCleared;

    public IReadOnlyList<Log> Logs => logs;
    List<Log> _deletedLogs;


    public bool IsRestorable => !_deletedLogs.IsNullOrEmpty();
    public bool HasLogs => !logs.IsNullOrEmpty();

    public void Restore()
    {
        if (!IsRestorable) return;
        logs = _deletedLogs;
        OnLogsChanged();
    }

    public void OnEnable()
    {
        if(!Application.isEditor || includeUnityLogsInEditorInEditor)
            Application.logMessageReceivedThreaded += UnityLog;
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += PlayModeChanged;
#endif
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= UnityLog;
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= PlayModeChanged;
#endif
    }

    public void Log(string message, Object source, LogTag[] tags, params object[] content) =>
        LogCustom(message, source, tags, content);

    public void Log(string message, Object source, LogTag tag, params object[] content) =>
        LogCustom(message, source, new[] {tag}, content);

    // Log Without Content
    public void Log(string message) =>
        LogCustom(message, null, null, null);

    public void Log(string message, params LogTag[] tags) =>
        LogCustom(message, null, tags, null);

    public void Log(string message, Object source, params LogTag[] tags) =>
        LogCustom(message, source, tags, null);

    // Without Message 
    public void Log(params object[] content) =>
        LogCustom(null, null, null, content);

    public void Log(LogTag tag, params object[] content) =>
        LogCustom(null, null, new[] {tag}, content);

    public void Log(LogTag[] tags, params object[] content) =>
        LogCustom(null, null, tags, content);

    public void Log(Object source, LogTag tag, params object[] objects) =>
        LogCustom(string.Empty, source, new[] {tag}, objects);

    public void Log(Object source, LogTag[] tags, params object[] objects) =>
        LogCustom(string.Empty, source, tags, objects);

    // Custom
    public void Log(PlayLog log) =>
        LogCustom(log.message, log.source, log.tags, log.content.ToArray());
 
    // Private Log Methods

    void LogCustom(string message, Object source, LogTag[] tags, object[] objects) =>
        LogFull(message, null, objects, LogType.PlayLog, source, tags);

    void UnityLog(string message, string stackTrace, UnityEngine.LogType logType)
    {
        // Don't record compiler warnings & errors.
        if ((logType == UnityEngine.LogType.Error || logType == UnityEngine.LogType.Warning)
            && string.IsNullOrEmpty(stackTrace)) return;

        LogFull(message, stackTrace, null, logType.ToCustomLogType(), null, null);
    }

    void LogFull(string message, string stackTrace, object[] content, LogType logType, Object source, LogTag[] tags)
    {
        var log = new Log(message, stackTrace, content, logType, logs.Count, tags, source);
        logs.Add(log);
        LogReceived?.Invoke(log);
        OnLogsChanged();
    }


    public void ClearLogs()
    {
        _deletedLogs = logs;
        logs = new List<Log>();
        LogsCleared?.Invoke();
        OnLogsChanged();
    }

    void OnLogsChanged()
    {
        SetObjectDirty();
        LogsChanged?.Invoke();
    }

    void SetObjectDirty()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

#if UNITY_EDITOR
    public int callbackOrder => 0;
    
    public void OnPreprocessBuild(BuildReport report)
    {
        if (clearObBuild)
            ClearLogs();
    }

    void PlayModeChanged(PlayModeStateChange obj)
    {
        if (obj == PlayModeStateChange.ExitingEditMode && clearOnPlay)
            ClearLogs();
    }
#endif
}
}