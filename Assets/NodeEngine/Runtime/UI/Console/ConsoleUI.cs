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
    [SerializeField] private TextMeshProUGUI collapseButtonText;
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

    [Header("Refresh Settings")]
    [SerializeField] private float delayedRefreshTime = 0.5f;

    private List<ConsoleEntry> allEntries = new List<ConsoleEntry>();
    private readonly List<ConsoleEntry> filteredEntries = new List<ConsoleEntry>();
    private bool isCollapsed = false;
    private string searchFilter = "";
    private bool showErrors = true;
    private bool showWarnings = true;
    private bool showLogs = true;


    private readonly Queue<ConsoleEntryUI> entryPool = new Queue<ConsoleEntryUI>();
    private readonly List<ConsoleEntryUI> activeEntries = new List<ConsoleEntryUI>();


    private float lastRefreshTime;
    private const float refreshInterval = 0.05f;

    private bool isConsoleVisible = false;
    private Coroutine delayedRefreshCoroutine;

    // Track last known entry for collapse mode
    private ConsoleEntry lastCollapsedEntry = null;

    public static ConsoleUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


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


        for (int i = 0; i < initialPoolSize; i++)
        {
            ConsoleEntryUI entry = Instantiate(entryPrefab, entriesParent);
            entry.gameObject.SetActive(false);
            entryPool.Enqueue(entry);
        }

        UpdateCollapseButtonText();
        SetConsoleVisibility(false);
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
            ForceRefresh();
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

    private void OnFilterChanged(bool _)
    {
        showErrors = errorToggle.isOn;
        showWarnings = warningToggle.isOn;
        showLogs = logToggle.isOn;
        ScheduleDelayedRefresh();
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
        if (isCollapsed && allEntries.Count > 0)
        {
            // Use the lastCollapsedEntry reference for more reliable collapsing
            if (lastCollapsedEntry != null &&
                lastCollapsedEntry.message == logString &&
                lastCollapsedEntry.logType == type)
            {
                // Add timestamp to existing entry
                lastCollapsedEntry.timestamps.Add(DateTime.Now);
                ScheduleDelayedRefresh();
                return;
            }

            // Check the actual last entry in the list as fallback
            var lastEntry = allEntries.Last();
            if (lastEntry.message == logString && lastEntry.logType == type)
            {
                lastEntry.timestamps.Add(DateTime.Now);
                lastCollapsedEntry = lastEntry;
                ScheduleDelayedRefresh();
                return;
            }
        }

        // Create new entry
        var newEntry = new ConsoleEntry
        {
            message = logString,
            stackTrace = stackTrace,
            logType = type
        };
        newEntry.timestamps.Add(DateTime.Now);

        allEntries.Add(newEntry);
        lastCollapsedEntry = newEntry;
        ScheduleDelayedRefresh();

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ScheduleDelayedRefresh()
    {
        if (delayedRefreshCoroutine != null)
        {
            StopCoroutine(delayedRefreshCoroutine);
        }

        delayedRefreshCoroutine = StartCoroutine(DelayedRefreshCoroutine());
    }

    private IEnumerator DelayedRefreshCoroutine()
    {
        yield return new WaitForSeconds(delayedRefreshTime);
        ForceRefresh();
    }

    public void ForceRefresh()
    {
        lastRefreshTime = 0f;
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

        if (filteredEntries.Count > maxDisplayedEntries)
        {
            filteredEntries.RemoveRange(0, filteredEntries.Count - maxDisplayedEntries);
        }
    }

    private void UpdateEntryUIs()
    {
        while (activeEntries.Count > filteredEntries.Count)
        {
            var entryToReturn = activeEntries[activeEntries.Count - 1];
            entryToReturn.gameObject.SetActive(false);
            entryPool.Enqueue(entryToReturn);
            activeEntries.RemoveAt(activeEntries.Count - 1);
        }

        for (int i = 0; i < filteredEntries.Count; i++)
        {
            ConsoleEntryUI entryUI;
            if (i < activeEntries.Count)
            {
                entryUI = activeEntries[i];
            }
            else
            {
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

            entryUI.transform.SetSiblingIndex(i);
            entryUI.Initialize(filteredEntries[i]);
        }
    }

    private void OnSearchChanged(string searchText)
    {
        searchFilter = searchText;
        ScheduleDelayedRefresh();
    }

    private void ToggleCollapse()
    {
        isCollapsed = !isCollapsed;
        lastCollapsedEntry = null; // Reset when changing collapse mode

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
        if (collapseButtonText != null)
        {
            collapseButtonText.text = isCollapsed ? "Collapse: ON" : "Collapse: OFF";
        }
    }

    private void GroupCollapsedEntries()
    {
        var groupedEntries = new Dictionary<int, ConsoleEntry>();
        var messageToIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in allEntries)
        {
            string messageKey = $"{entry.logType}|{entry.message}";

            if (messageToIdMap.TryGetValue(messageKey, out int existingId))
            {
                // Add to existing grouped entry
                if (groupedEntries.TryGetValue(existingId, out var existingEntry))
                {
                    // Create new list to avoid reference issues
                    var combinedTimestamps = new List<DateTime>(existingEntry.timestamps);
                    combinedTimestamps.AddRange(entry.timestamps);
                    existingEntry.timestamps = combinedTimestamps;
                }
            }
            else
            {
                // Create new grouped entry with the original ID
                var newGroupedEntry = new ConsoleEntry(entry); // Use copy constructor
                groupedEntries[entry.id] = newGroupedEntry;
                messageToIdMap[messageKey] = entry.id;
            }
        }

        allEntries = new List<ConsoleEntry>(groupedEntries.Values);

        // Update lastCollapsedEntry reference
        if (allEntries.Count > 0)
        {
            lastCollapsedEntry = allEntries.Last();
        }
    }

    private void ExpandCollapsedEntries()
    {
        var expandedEntries = new List<ConsoleEntry>();

        foreach (var entry in allEntries)
        {
            foreach (var timestamp in entry.timestamps)
            {
                // Create new entry for each timestamp with proper ID inheritance
                var newEntry = new ConsoleEntry
                {
                    id = entry.id, // Keep the same ID for tracking
                    message = entry.message,
                    stackTrace = entry.stackTrace,
                    logType = entry.logType
                };
                newEntry.timestamps.Add(timestamp);
                expandedEntries.Add(newEntry);
            }
        }

        allEntries = expandedEntries;

        // Update lastCollapsedEntry reference
        if (allEntries.Count > 0)
        {
            lastCollapsedEntry = allEntries.Last();
        }
    }

    public void Clear()
    {
        allEntries.Clear();
        filteredEntries.Clear();
        lastCollapsedEntry = null;
        ForceRefresh();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Application.logMessageReceived -= HandleLog;

            if (delayedRefreshCoroutine != null)
            {
                StopCoroutine(delayedRefreshCoroutine);
            }
        }
    }
}