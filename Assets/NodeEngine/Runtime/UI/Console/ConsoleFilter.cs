using System;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleFilter
{
    public bool ShowErrors { get; set; } = true;
    public bool ShowWarnings { get; set; } = true;
    public bool ShowLogs { get; set; } = true;
    public string SearchText { get; set; } = "";

    public List<ConsoleEntry> Filter(IEnumerable<ConsoleEntry> entries, int maxDisplayed = 200)
    {
        var filtered = new List<ConsoleEntry>();

        if (!ShowErrors && !ShowWarnings && !ShowLogs)
            return filtered;

        foreach (var entry in entries)
        {
            bool typeMatches = entry.logType switch
            {
                LogType.Error or LogType.Exception => ShowErrors,
                LogType.Warning => ShowWarnings,
                LogType.Log => ShowLogs,
                _ => false
            };

            if (!typeMatches) continue;
            if (!string.IsNullOrEmpty(SearchText) &&
                !entry.message.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                continue;

            filtered.Add(entry);
        }

        if (filtered.Count > maxDisplayed)
            filtered.RemoveRange(0, filtered.Count - maxDisplayed);

        return filtered;
    }
}