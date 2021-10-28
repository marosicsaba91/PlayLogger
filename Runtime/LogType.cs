using System;

namespace PlayLogging
{
public enum LogType
{
    UnityError = 1,
    UnityAssert = 2,
    UnityWarning = 4,
    UnityLog = 8,
    Exception = 16,
    PlayLog = 32
}

public static class LogTypeHelper
{

    public static LogType ToCustomLogType(this UnityEngine.LogType original)
    {
        switch (original)
        {
            case UnityEngine.LogType.Error:
                return LogType.UnityError;
            case UnityEngine.LogType.Assert:
                return LogType.UnityAssert;
            case UnityEngine.LogType.Warning:
                return LogType.UnityWarning;
            case UnityEngine.LogType.Log:
                return LogType.UnityLog;
            case UnityEngine.LogType.Exception:
                return LogType.Exception;
            default:
                throw new ArgumentOutOfRangeException(nameof(original), original, null);
        }
    }

    public const int all = 
        (int) LogType.PlayLog |(int) LogType.UnityLog | (int) LogType.UnityWarning |
        (int) LogType.UnityAssert |(int) LogType.UnityError | (int) LogType.Exception;
}

}
