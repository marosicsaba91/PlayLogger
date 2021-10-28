using System.Collections.Generic;
using UnityEngine;

namespace PlayLogging
{
public class PlayLog
{
    internal string message;
    internal Object source;
    internal LogTag[] tags;
    internal List<object> content;
    
    public PlayLog(string message, params LogTag[] tags)
    {
        this.message = message; 
        this.tags = tags;
    }
    
    public PlayLog(string message, Object source, params LogTag[] tags)
    {
        this.message = message;
        this.source = source;
        this.tags = tags;
    }

    public void AddContent(Object content)
    {
        if (this.content == null)
            this.content = new List<object>();
        this.content.Add(content); 
    } 
}
}