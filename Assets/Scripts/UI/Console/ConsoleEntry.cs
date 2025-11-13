using System;
using UnityEngine;

[Serializable]
public class ConsoleEntry
{
    public string message;
    public string stackTrace;
    public LogType logType;
    public DateTime timestamp;
    public int count = 1;

    public string DisplayMessage => count > 1 ? $"{message} ({count})" : message;
    public string TimeString => timestamp.ToString("HH:mm:ss");

    public Color Color => logType switch
    {
        LogType.Error => new Color(1f, 0.4f, 0.4f),
        LogType.Warning => new Color(1f, 0.8f, 0.4f),
        LogType.Log => Color.white,
        LogType.Exception => new Color(1f, 0.2f, 0.2f),
        _ => Color.white
    };
}