using System;
using System.Text;
using MUtility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayLogging
{
[Serializable]
public class Log
{
    [SerializeField] LogType logType = LogType.PlayLog;
    [SerializeField] string message = default;
    [SerializeField] string stackTrace = default;
    [SerializeField] int index = default;
    [SerializeField] long dateTimeInTicks = default;
    [SerializeField] float gameTime = default;
    [SerializeField] int frameCount = default;
    [SerializeField] float unscaledTime = default;
    [SerializeField] float fixedTime = default;
    [SerializeField] float fixedUnscaledTime = default;
    [SerializeField] LogTag[] tags = default;
    [SerializeField] Object source = default;
    [SerializeField] string sourceName = default;
    [SerializeField] string[] content = default;

    public Object Source => source;
    public string SourceName => sourceName;
    public string StackTrace => stackTrace;
    public string RealTimeShortString => TicksToShortString(dateTimeInTicks);
    public string GameTimeShortString => SecondsToShortString(gameTime);
    public string UnscaledTimeShortString => SecondsToShortString(unscaledTime);
    public string FixedTimeShortString => SecondsToShortString(fixedTime);
    public string FixedUnscaledTimeShortString => SecondsToShortString(fixedUnscaledTime);

    static readonly StringBuilder builder = new StringBuilder();
    public string ContentString(bool separatedLines) => ContentToString(content, separatedLines);

    public LogTag[] Tags => tags;
    public LogType LogType => logType;
    public int Index => index;
    public string Message => message;
    public string[] Content => content;

    public string MessageAndContent
    {
        get
        {
            if (string.IsNullOrEmpty(message))
                return ContentString(false);
            if (content.IsNullOrEmpty())
                return message;
            return $"{message} :   \t{ContentString(false)}";
        }
    }

    public int FrameCount => frameCount;

    internal Log(string message, string stackTrace, object[] content, LogType logType, int index, LogTag[] tags,
        Object source)
    {
        this.message = message;
        if (stackTrace != null && stackTrace.EndsWith("\n"))
            stackTrace = stackTrace.Substring(
                0, stackTrace.LastIndexOf("\n", StringComparison.Ordinal));

        this.stackTrace = stackTrace;

        this.content = ObjectsToContent(content);
        this.logType = logType;
        this.index = index;
        this.tags = tags.CreateCopy();
        this.source = source;
        if (source != null)
            sourceName = $"{source.name} ({source.GetType().Name})";
        SetTime();
    }

    void SetTime()
    {
        dateTimeInTicks = DateTime.Now.Ticks;
        if (!Application.isPlaying) return;
        gameTime = Time.time;
        frameCount = Time.frameCount;
        unscaledTime = Time.unscaledTime;
        fixedTime = Time.fixedTime;
        fixedUnscaledTime = Time.fixedUnscaledTime;
    }


    string TicksToShortString(long ticks)
    {
        var time = new DateTime(ticks);
        int hours = time.Hour;
        int minutes = time.Minute;
        float seconds = time.Second + (time.Millisecond / 1000f);
        return ToHeaderShortString(hours, minutes, seconds);
    }

    string SecondsToShortString(float seconds)
    {
        int hours = (int) seconds / 60 / 60;
        seconds -= hours * 60 * 60;
        int minutes = (int) seconds / 60;
        seconds -= minutes * 60;
        return ToHeaderShortString(hours, minutes, seconds);
    }

    string ToHeaderShortString(int hours, int minutes, float seconds)
    {
        if (hours != 0)
            return $"{hours}: {minutes:00}: {seconds:00}";
        if (minutes != 0)
            return $"{minutes:00}: {seconds:00.00}";
        if (seconds != 0)
            return $"{seconds:0.00}";
        return "0";
    }


    static string[] ObjectsToContent(object[] content)
    {
        if (content == null) return null;

        var result = new string[content.Length];
        for (var i = 0; i < content.Length; i++)
            result[i] = ObjectToContentRow(content[i]);
        return result;
    }

    static string ContentToString(string[] content, bool separatedLines)
    {
        if (content == null) return string.Empty;

        builder.Clear();
        for (var i = 0; i < content.Length; i++)
        {
            string row = content[i];
            builder.Append(ContentRowToValueString(row));
            if(i == content.Length-1) break;
            builder.Append(separatedLines ? ",\n" : ",   ");
        }

        return builder.ToString();
    }

    static string ObjectToContentRow(object obj) => $"[{obj.GetType()}] : {obj}";

    static string ContentRowToValueString(string contentRow)
    {
        const string separatorString = "] : ";
        int index = contentRow.IndexOf(separatorString, StringComparison.Ordinal);
        if (index <= -1)
            return "Corrupted";
        index += separatorString.Length;
        
        return contentRow.Substring(index, contentRow.Length - index);
    }
}
}