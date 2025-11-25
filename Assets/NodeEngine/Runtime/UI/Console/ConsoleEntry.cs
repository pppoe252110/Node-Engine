using System;
using System.Collections.Generic;  
using UnityEngine;

[Serializable]
public struct ConsoleEntry  
{
    public string message;
    public string stackTrace;
    public LogType logType;
    public List<DateTime> timestamps;  
    public int count => timestamps?.Count ?? 0;  

    public string DisplayMessage => count > 1 ? $"{message} ({count})" : message;
    public string TimeString => timestamps != null && timestamps.Count > 0 ? timestamps[0].ToString("HH:mm:ss") : "";  

    public Color Color => logType switch
    {
        LogType.Error => new Color(1f, 0.4f, 0.4f),
        LogType.Warning => new Color(1f, 0.8f, 0.4f),
        LogType.Log => Color.white,
        LogType.Exception => new Color(1f, 0.2f, 0.2f),
        _ => Color.white
    };
}