using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleUI : BasePanel
{
    [Header("References")]
    [SerializeField] private Transform _entriesParent;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private TMP_InputField _searchInput;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _collapseButton;
    [SerializeField] private TextMeshProUGUI _collapseButtonText;
    [SerializeField] private Toggle _errorToggle;
    [SerializeField] private Toggle _warningToggle;
    [SerializeField] private Toggle _logToggle;

    [Header("Prefabs")]
    [SerializeField] private ConsoleEntryUI _entryPrefab;

    [Header("Pooling")]
    [SerializeField] private int _initialPoolSize = 50;
    [SerializeField] private int _maxDisplayedEntries = 200;

    [Header("Refresh Settings")]
    [SerializeField] private float _delayedRefreshTime = 0.5f;

    private ConsoleLogManager _logManager;
    private ConsoleFilter _filter;
    private readonly Queue<ConsoleEntryUI> _entryPool = new();
    private readonly List<ConsoleEntryUI> _activeEntries = new();
    private Coroutine _delayedRefreshCoroutine;

    public static ConsoleUI Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _logManager = new ConsoleLogManager();
        _filter = new ConsoleFilter();

        _logManager.OnEntriesChanged += ScheduleDelayedRefresh;

        Application.logMessageReceived += HandleLog;
        SetupUI();

        base.Awake();
    }

    private void SetupUI()
    {
        _clearButton.onClick.AddListener(() => _logManager.Clear());
        _collapseButton.onClick.AddListener(ToggleCollapse);
        _searchInput.onValueChanged.AddListener(OnSearchChanged);
        _errorToggle.onValueChanged.AddListener(OnFilterChanged);
        _warningToggle.onValueChanged.AddListener(OnFilterChanged);
        _logToggle.onValueChanged.AddListener(OnFilterChanged);

        for (int i = 0; i < _initialPoolSize; i++)
        {
            var entry = Instantiate(_entryPrefab, _entriesParent);
            entry.gameObject.SetActive(false);
            _entryPool.Enqueue(entry);
        }

        UpdateCollapseButtonText();
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logManager.AddEntry(logString, stackTrace, type);
        StartCoroutine(ScrollToBottom());
    }

    private void OnFilterChanged(bool _)
    {
        _filter.ShowErrors = _errorToggle.isOn;
        _filter.ShowWarnings = _warningToggle.isOn;
        _filter.ShowLogs = _logToggle.isOn;
        ScheduleDelayedRefresh();
    }

    private void OnSearchChanged(string text)
    {
        _filter.SearchText = text;
        ScheduleDelayedRefresh();
    }

    private void ToggleCollapse()
    {
        _logManager.SetCollapsed(!_logManager.IsCollapsed);
        UpdateCollapseButtonText();
    }

    private void UpdateCollapseButtonText()
    {
        _collapseButtonText.text = _logManager.IsCollapsed ? "Collapse: ON" : "Collapse: OFF";
    }

    private void ScheduleDelayedRefresh()
    {
        if (_delayedRefreshCoroutine != null)
            StopCoroutine(_delayedRefreshCoroutine);
        _delayedRefreshCoroutine = StartCoroutine(DelayedRefreshCoroutine());
    }

    private IEnumerator DelayedRefreshCoroutine()
    {
        yield return new WaitForSeconds(_delayedRefreshTime);
        RefreshUI();
    }

    private void RefreshUI()
    {
        var filteredEntries = _filter.Filter(_logManager.AllEntries, _maxDisplayedEntries);
        UpdateEntryUIs(filteredEntries);
    }

    private void UpdateEntryUIs(List<ConsoleEntry> filteredEntries)
    {
        // Return excess entries to pool
        while (_activeEntries.Count > filteredEntries.Count)
        {
            var entry = _activeEntries[^1];
            entry.gameObject.SetActive(false);
            _entryPool.Enqueue(entry);
            _activeEntries.RemoveAt(_activeEntries.Count - 1);
        }

        // Update or create entries
        for (int i = 0; i < filteredEntries.Count; i++)
        {
            ConsoleEntryUI entryUI;
            if (i < _activeEntries.Count)
            {
                entryUI = _activeEntries[i];
            }
            else
            {
                entryUI = _entryPool.Count > 0
                    ? _entryPool.Dequeue()
                    : Instantiate(_entryPrefab, _entriesParent);
                entryUI.gameObject.SetActive(true);
                _activeEntries.Add(entryUI);
            }

            entryUI.transform.SetSiblingIndex(i);
            entryUI.Initialize(filteredEntries[i]);
        }
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        _scrollRect.verticalNormalizedPosition = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Application.logMessageReceived -= HandleLog;
            _logManager.OnEntriesChanged -= ScheduleDelayedRefresh;
        }
    }
}