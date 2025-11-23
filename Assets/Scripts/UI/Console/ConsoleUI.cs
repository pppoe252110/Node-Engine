using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private TextMeshProUGUI collapseButtonText; // <-- CORRECTED: Added a field for the button's text
    [SerializeField] private Toggle errorToggle;
    [SerializeField] private Toggle warningToggle;
    [SerializeField] private Toggle logToggle;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Prefabs")]
    [SerializeField] private ConsoleEntryUI entryPrefab;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int maxDisplayedEntries = 200;

    [Header("Fading")]
    [SerializeField] private float fadeDuration = 0.3f;

    // FIX: Removed 'readonly' from allEntries to allow reassignment in collapse/expand methods.
    private List<ConsoleEntry> allEntries = new List<ConsoleEntry>();
    private readonly List<ConsoleEntry> filteredEntries = new List<ConsoleEntry>();
    private bool isCollapsed = false;
    private string searchFilter = "";
    private bool showErrors = true;
    private bool showWarnings = true;
    private bool showLogs = true;

    // Pooling for UI entries
    private readonly Queue<ConsoleEntryUI> entryPool = new Queue<ConsoleEntryUI>();
    private readonly List<ConsoleEntryUI> activeEntries = new List<ConsoleEntryUI>();

    // Throttling for refreshes
    private float lastRefreshTime;
    private const float refreshInterval = 0.05f; // Reduced for more responsive UI

    private bool isConsoleVisible = false;

    public static ConsoleUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure canvasGroup exists
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

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

        // Initialize entry pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            ConsoleEntryUI entry = Instantiate(entryPrefab, entriesParent);
            entry.gameObject.SetActive(false);
            entryPool.Enqueue(entry);
        }

        UpdateCollapseButtonText();
        SetConsoleVisibility(false); // Start hidden
    }

    public void ToggleConsoleVisibility()
    {
        SetConsoleVisibility(!isConsoleVisible);
    }

    public void SetConsoleVisibility(bool isVisible)
    {
        if (isConsoleVisible == isVisible) return;

        isConsoleVisible = isVisible;
        StopAllCoroutines();
        StartCoroutine(FadeConsole(isVisible));

        if (isVisible)
        {
            ForceRefresh(); // Refresh content when console becomes visible
        }
    }

    private IEnumerator FadeConsole(bool fadeIn)
    {
        if (fadeIn)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        float startAlpha = canvasGroup.alpha;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.interactable = fadeIn;
        canvasGroup.blocksRaycasts = fadeIn;

        if (!fadeIn)
        {
            canvasGroup.gameObject.SetActive(false);
        }
    }

    private void OnFilterChanged(bool _) // The 'isOn' value is not needed, we just check all toggles
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

        if (isCollapsed && allEntries.Count > 0)
        {
            var lastEntry = allEntries.Last();
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
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame(); // Wait for UI to update
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ForceRefresh()
    {
        lastRefreshTime = 0f; // Reset timer to bypass throttle
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (Time.time - lastRefreshTime < refreshInterval) return;
        lastRefreshTime = Time.time;

        FilterEntries();
        UpdateEntryUIs();
    }

    private void FilterEntries()
    {
        filteredEntries.Clear();

        // Early exit if no log types are selected
        if (!showErrors && !showWarnings && !showLogs) return;

        foreach (var entry in allEntries)
        {
            bool typeMatches = entry.logType switch
            {
                LogType.Error or LogType.Exception => showErrors,
                LogType.Warning => showWarnings,
                LogType.Log => showLogs,
                _ => false
            };

            if (!typeMatches) continue;
            if (!string.IsNullOrEmpty(searchFilter) && !entry.message.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)) continue;

            filteredEntries.Add(entry);
        }

        // Limit to max displayed entries (keep the most recent)
        if (filteredEntries.Count > maxDisplayedEntries)
        {
            filteredEntries.RemoveRange(0, filteredEntries.Count - maxDisplayedEntries);
        }
    }

    private void UpdateEntryUIs()
    {
        // Deactivate excess entries that are no longer needed
        while (activeEntries.Count > filteredEntries.Count)
        {
            var entryToReturn = activeEntries[activeEntries.Count - 1];
            entryToReturn.gameObject.SetActive(false);
            entryPool.Enqueue(entryToReturn);
            activeEntries.RemoveAt(activeEntries.Count - 1);
        }

        // Activate or create entries for the filtered list
        for (int i = 0; i < filteredEntries.Count; i++)
        {
            ConsoleEntryUI entryUI;
            if (i < activeEntries.Count)
            {
                // Reuse an existing active entry
                entryUI = activeEntries[i];
            }
            else
            {
                // Need a new entry, get from pool or instantiate
                if (entryPool.Count > 0)
                {
                    entryUI = entryPool.Dequeue();
                }
                else
                {
                    entryUI = Instantiate(entryPrefab, entriesParent);
                }
                entryUI.gameObject.SetActive(true);
                activeEntries.Add(entryUI);
            }

            // Ensure the entry is at the correct position in the hierarchy
            entryUI.transform.SetSiblingIndex(i);
            entryUI.Initialize(filteredEntries[i]);
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

        UpdateCollapseButtonText();
        ForceRefresh();
    }

    private void UpdateCollapseButtonText()
    {
        // CORRECTED: Directly set the text on the serialized field.
        if (collapseButtonText != null)
        {
            collapseButtonText.text = isCollapsed ? "Collapse: ON" : "Collapse: OFF";
        }
    }

    private void GroupCollapsedEntries()
    {
        var groupedEntries = new Dictionary<string, ConsoleEntry>(StringComparer.Ordinal);

        foreach (var entry in allEntries)
        {
            string key = $"{entry.logType}|{entry.message}";
            if (groupedEntries.TryGetValue(key, out var existingEntry))
            {
                existingEntry.timestamps.AddRange(entry.timestamps);
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
        filteredEntries.Clear();
        RefreshUI(); // This will trigger UpdateEntryUIs to deactivate everything
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}