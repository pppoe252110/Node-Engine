using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConsoleLogManager
{
    private List<ConsoleEntry> _allEntries = new();
    private bool _isCollapsed = false;
    private ConsoleEntry _lastCollapsedEntry = null;

    public IReadOnlyList<ConsoleEntry> AllEntries => _allEntries;
    public event Action OnEntriesChanged;

    public void AddEntry(string message, string stackTrace, LogType type)
    {
        if (_isCollapsed && _allEntries.Count > 0)
        {
            if (_lastCollapsedEntry != null &&
                _lastCollapsedEntry.message == message &&
                _lastCollapsedEntry.logType == type)
            {
                _lastCollapsedEntry.timestamps.Add(DateTime.Now);
                OnEntriesChanged?.Invoke();
                return;
            }

            var lastEntry = _allEntries.Last();
            if (lastEntry.message == message && lastEntry.logType == type)
            {
                lastEntry.timestamps.Add(DateTime.Now);
                _lastCollapsedEntry = lastEntry;
                OnEntriesChanged?.Invoke();
                return;
            }
        }

        var newEntry = new ConsoleEntry
        {
            message = message,
            stackTrace = stackTrace,
            logType = type
        };
        newEntry.timestamps.Add(DateTime.Now);

        _allEntries.Add(newEntry);
        _lastCollapsedEntry = newEntry;
        OnEntriesChanged?.Invoke();
    }

    public void Clear()
    {
        _allEntries.Clear();
        _lastCollapsedEntry = null;
        OnEntriesChanged?.Invoke();
    }

    public void SetCollapsed(bool collapsed)
    {
        if (_isCollapsed == collapsed) return;
        _isCollapsed = collapsed;
        _lastCollapsedEntry = null;

        if (_isCollapsed)
            GroupCollapsedEntries();
        else
            ExpandCollapsedEntries();

        OnEntriesChanged?.Invoke();
    }

    public bool IsCollapsed => _isCollapsed;

    private void GroupCollapsedEntries()
    {
        var groupedEntries = new Dictionary<int, ConsoleEntry>();
        var messageToIdMap = new Dictionary<string, int>();

        foreach (var entry in _allEntries)
        {
            string messageKey = $"{entry.logType}|{entry.message}";
            if (messageToIdMap.TryGetValue(messageKey, out int existingId))
            {
                if (groupedEntries.TryGetValue(existingId, out var existingEntry))
                {
                    var combined = new List<DateTime>(existingEntry.timestamps);
                    combined.AddRange(entry.timestamps);
                    existingEntry.timestamps = combined;
                }
            }
            else
            {
                var newGroupedEntry = new ConsoleEntry(entry);
                groupedEntries[entry.id] = newGroupedEntry;
                messageToIdMap[messageKey] = entry.id;
            }
        }

        _allEntries = new List<ConsoleEntry>(groupedEntries.Values);
        if (_allEntries.Count > 0)
            _lastCollapsedEntry = _allEntries.Last();
    }

    private void ExpandCollapsedEntries()
    {
        var expandedEntries = new List<ConsoleEntry>();
        foreach (var entry in _allEntries)
        {
            foreach (var timestamp in entry.timestamps)
            {
                var newEntry = new ConsoleEntry
                {
                    id = entry.id,
                    message = entry.message,
                    stackTrace = entry.stackTrace,
                    logType = entry.logType
                };
                newEntry.timestamps.Add(timestamp);
                expandedEntries.Add(newEntry);
            }
        }
        _allEntries = expandedEntries;
        if (_allEntries.Count > 0)
            _lastCollapsedEntry = _allEntries.Last();
    }
}