using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entriesParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button collapseButton;
    [SerializeField] private Toggle errorToggle;
    [SerializeField] private Toggle warningToggle;
    [SerializeField] private Toggle logToggle;

    [Header("Prefabs")]
    [SerializeField] private ConsoleEntryUI entryPrefab;  // Now directly ConsoleEntryUI

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 50;  // Pre-create some entry objects
    [SerializeField] private int maxDisplayedEntries = 200;  // Limit to prevent FPS drops

    private List<ConsoleEntry> allEntries = new List<ConsoleEntry>();
    private List<ConsoleEntry> filteredEntries = new List<ConsoleEntry>();
    private bool isCollapsed = false;
    private string searchFilter = "";
    private bool showErrors = true;
    private bool showWarnings = true;
    private bool showLogs = true;

    // Pooling for UI entries (now directly ConsoleEntryUI)
    private Queue<ConsoleEntryUI> entryPool = new Queue<ConsoleEntryUI>();

    // Throttling for refreshes
    private float lastRefreshTime;
    private const float refreshInterval = 0.2f;  // Update UI every 0.2 seconds

    public static ConsoleUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Application.logMessageReceived += HandleLog;
        SetupUI();
    }

    private void SetupUI()
    {
        clearButton.onClick.AddListener(Clear);
        collapseButton.onClick.AddListener(ToggleCollapse);
        searchInput.onValueChanged.AddListener(OnSearchChanged);

        errorToggle.onValueChanged.AddListener(OnFilterChanged);
        warningToggle.onValueChanged.AddListener(OnFilterChanged);
        logToggle.onValueChanged.AddListener(OnFilterChanged);

        // Set initial toggle states
        errorToggle.isOn = true;
        warningToggle.isOn = true;
        logToggle.isOn = true;

        // Initialize entry pool (directly instantiate ConsoleEntryUI)
        for (int i = 0; i < initialPoolSize; i++)
        {
            ConsoleEntryUI obj = Instantiate(entryPrefab, entriesParent);
            obj.gameObject.SetActive(false);
            entryPool.Enqueue(obj);
        }

        // Ensure collapse button reflects initial state
        collapseButton.GetComponentInChildren<TextMeshProUGUI>().text = isCollapsed ? "Collapse: ON" : "Collapse: OFF";
    }

    private void OnFilterChanged(bool isOn)
    {
        showErrors = errorToggle.isOn;
        showWarnings = warningToggle.isOn;
        showLogs = logToggle.isOn;
        RefreshUI();
    }

    public void LogMessage(string message, LogType type = LogType.Log)
    {
        HandleLog(message, "", type);
    }

    public void LogWarning(string message)
    {
        HandleLog(message, "", LogType.Warning);
    }

    public void LogError(string message)
    {
        HandleLog(message, "", LogType.Error);
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        var newEntry = new ConsoleEntry
        {
            message = logString,
            stackTrace = stackTrace,
            logType = type,
            timestamps = new List<DateTime> { DateTime.Now }
        };

        // Only collapse consecutive if collapse is enabled
        if (isCollapsed && allEntries.Count > 0)
        {
            var lastEntry = allEntries[allEntries.Count - 1];
            if (lastEntry.message == logString && lastEntry.logType == type)
            {
                lastEntry.timestamps.Add(DateTime.Now);
                RefreshUI();
                return;
            }
        }

        allEntries.Add(newEntry);
        RefreshUI();

        // Auto-scroll to bottom when new messages arrive
        if (scrollRect.verticalNormalizedPosition <= 0.01f)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void RefreshUI()
    {
        // Throttle refreshes to avoid FPS drops
        if (Time.time - lastRefreshTime < refreshInterval) return;
        lastRefreshTime = Time.time;

        FilterEntries();
        UpdateEntryUIs();
    }

    private void FilterEntries()
    {
        filteredEntries.Clear();

        foreach (var entry in allEntries)
        {
            // Check if this log type should be shown
            bool shouldShow = false;
            switch (entry.logType)
            {
                case LogType.Error:
                case LogType.Exception:
                    shouldShow = showErrors;
                    break;
                case LogType.Warning:
                    shouldShow = showWarnings;
                    break;
                case LogType.Log:
                    shouldShow = showLogs;
                    break;
            }

            if (!shouldShow) continue;
            if (!string.IsNullOrEmpty(searchFilter) &&
                !entry.message.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)) continue;

            filteredEntries.Add(entry);
        }

        // Limit to max displayed entries (keep the most recent)
        if (filteredEntries.Count > maxDisplayedEntries)
        {
            filteredEntries = filteredEntries.GetRange(filteredEntries.Count - maxDisplayedEntries, maxDisplayedEntries);
        }
    }

    private void UpdateEntryUIs()
    {
        // Return all active entries to the pool
        foreach (Transform child in entriesParent)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                entryPool.Enqueue(child.GetComponent<ConsoleEntryUI>());  // Still need GetComponent here for existing children, but it's minimal
            }
        }

        // Activate pooled entries for filtered list (no GetComponent needed)
        foreach (var entry in filteredEntries)
        {
            ConsoleEntryUI entryUI;
            if (entryPool.Count > 0)
            {
                entryUI = entryPool.Dequeue();
            }
            else
            {
                // Expand pool if needed (directly instantiate ConsoleEntryUI)
                entryUI = Instantiate(entryPrefab, entriesParent);
            }

            entryUI.gameObject.SetActive(true);
            entryUI.Initialize(entry);
        }
    }

    private void OnSearchChanged(string searchText)
    {
        searchFilter = searchText;
        RefreshUI();
    }

    private void ToggleCollapse()
    {
        isCollapsed = !isCollapsed;

        if (isCollapsed)
        {
            GroupCollapsedEntries();
        }
        else
        {
            ExpandCollapsedEntries();
        }

        collapseButton.GetComponentInChildren<TextMeshProUGUI>().text =
            isCollapsed ? "Collapse: ON" : "Collapse: OFF";
        RefreshUI();
    }

    private void GroupCollapsedEntries()
    {
        var groupedEntries = new Dictionary<string, ConsoleEntry>();

        foreach (var entry in allEntries)
        {
            string key = entry.message + "|" + entry.logType.ToString();

            if (groupedEntries.ContainsKey(key))
            {
                groupedEntries[key].timestamps.AddRange(entry.timestamps);
            }
            else
            {
                groupedEntries[key] = new ConsoleEntry
                {
                    message = entry.message,
                    stackTrace = entry.stackTrace,
                    logType = entry.logType,
                    timestamps = new List<DateTime>(entry.timestamps)
                };
            }
        }

        allEntries = new List<ConsoleEntry>(groupedEntries.Values);
    }

    private void ExpandCollapsedEntries()
    {
        var expandedEntries = new List<ConsoleEntry>();

        foreach (var entry in allEntries)
        {
            foreach (var timestamp in entry.timestamps)
            {
                expandedEntries.Add(new ConsoleEntry
                {
                    message = entry.message,
                    stackTrace = entry.stackTrace,
                    logType = entry.logType,
                    timestamps = new List<DateTime> { timestamp }
                });
            }
        }

        allEntries = expandedEntries;
    }

    public void Clear()
    {
        allEntries.Clear();

        // Deactivate and return all current entries to pool (maintains pooling)
        foreach (Transform child in entriesParent)
        {
            child.gameObject.SetActive(false);
            entryPool.Enqueue(child.GetComponent<ConsoleEntryUI>());  // Minimal GetComponent for cleanup
        }

        // Reset refresh timer
        lastRefreshTime = 0f;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
