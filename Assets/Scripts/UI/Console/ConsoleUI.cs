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
    [SerializeField] private GameObject entryPrefab;

    private List<ConsoleEntry> allEntries = new List<ConsoleEntry>();
    private List<ConsoleEntry> filteredEntries = new List<ConsoleEntry>();
    private bool isCollapsed = false;
    private string searchFilter = "";
    // Change this to track each type separately instead of using flags
    private bool showErrors = true;
    private bool showWarnings = true;
    private bool showLogs = true;

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
            timestamp = DateTime.Now
        };

        // Only collapse if collapse is enabled
        if (isCollapsed && allEntries.Count > 0)
        {
            var lastEntry = allEntries[allEntries.Count - 1];
            if (lastEntry.message == logString && lastEntry.logType == type)
            {
                lastEntry.count++;
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
    }

    private void UpdateEntryUIs()
    {
        // Clear existing entries
        foreach (Transform child in entriesParent)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        foreach (var entry in filteredEntries)
        {
            var entryUI = Instantiate(entryPrefab, entriesParent).GetComponent<ConsoleEntryUI>();
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

        // When turning OFF collapse, we need to create separate entries for any collapsed ones
        if (!isCollapsed)
        {
            ExpandCollapsedEntries();
        }

        collapseButton.GetComponentInChildren<TextMeshProUGUI>().text =
            isCollapsed ? "Collapse: ON" : "Collapse: OFF";
        RefreshUI();
    }

    private void ExpandCollapsedEntries()
    {
        // Create a new list to replace the collapsed entries
        var expandedEntries = new List<ConsoleEntry>();

        foreach (var entry in allEntries)
        {
            // If the entry was collapsed (count > 1), we need to expand it
            if (entry.count > 1)
            {
                // Add the entry once with count = 1
                expandedEntries.Add(new ConsoleEntry
                {
                    message = entry.message,
                    stackTrace = entry.stackTrace,
                    logType = entry.logType,
                    timestamp = entry.timestamp,
                    count = 1
                });
            }
            else
            {
                // Add the entry as-is
                expandedEntries.Add(entry);
            }
        }

        allEntries = expandedEntries;
    }

    public void Clear()
    {
        allEntries.Clear();
        RefreshUI();
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}