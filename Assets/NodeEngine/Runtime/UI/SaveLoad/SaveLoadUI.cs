using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

public class SaveLoadUI : BasePanel
{
    [Header("Save Section")]
    [SerializeField] private TMP_InputField _saveNameInput;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _quickSaveButton;
    [SerializeField] private TMP_Text _saveStatusText;

    [Header("Load Section")]
    [SerializeField] private Transform _saveFilesContainer;
    [SerializeField] private SaveFileEntryUI _saveFileEntryPrefab;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Button _quickLoadButton;
    [SerializeField] private TMP_Text _loadStatusText;

    [Header("Management")]
    [SerializeField] private Button _deleteAllSavesButton;
    [SerializeField] private TMP_Text _managementStatusText;

    private List<SaveFileEntryUI> _saveFileEntries = new List<SaveFileEntryUI>();
    private float _statusDisplayTimer;
    private bool _isPanelOpen = false;

    private GraphSaveLoadCoordinator _coordinator;

    [Inject]
    public void Construct(GraphSaveLoadCoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    private void Start()
    {
        SetupUI();

        RefreshSaveFilesList();
        _coordinator.OnGraphSaved += OnGraphSaved;
        _coordinator.OnGraphLoaded += OnGraphLoaded;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        PanelManager.Instance.RegisterPanel(this);
    }

    private void OnDestroy()
    {
        if (_coordinator != null)
        {
            _coordinator.OnGraphSaved -= OnGraphSaved;
            _coordinator.OnGraphLoaded -= OnGraphLoaded;
        }
    }

    private void Update()
    {
        HandleHotkeys();
        UpdateStatusTimers();
    }

    protected override void OnPanelOpenedAction()
    {
            RefreshSaveFilesList();
    }

    private void SetupUI()
    {
        _saveButton.onClick.AddListener(SaveGraph);
        _quickSaveButton.onClick.AddListener(QuickSave);
        _saveNameInput.onSubmit.AddListener((text) => SaveGraph());
        _saveNameInput.onValueChanged.AddListener(OnSaveNameChanged);

        _refreshButton.onClick.AddListener(RefreshSaveFilesList);
        _quickLoadButton.onClick.AddListener(QuickLoad);

        _deleteAllSavesButton.onClick.AddListener(DeleteAllSaves);

        UpdateSaveStatus("Ready to save");
        UpdateLoadStatus($"{GetSaveFilesCount()} save files");
        UpdateManagementStatus("");

        ClosePanel();
    }

    private void HandleHotkeys()
    {

        if (Keyboard.current.f5Key.wasReleasedThisFrame) QuickSave();
        else if (Keyboard.current.f9Key.wasReleasedThisFrame) QuickLoad();
        else if (Keyboard.current.escapeKey.wasReleasedThisFrame && _isPanelOpen) ClosePanel();
    }

    public void SaveGraph()
    {
        if (string.IsNullOrEmpty(_saveNameInput.text))
        {
            UpdateSaveStatus("Please enter a save name", 3f);
            return;
        }

        UpdateSaveStatus("Saving...", 0f);
        _coordinator.SaveGraph(_saveNameInput.text);
    }

    public void QuickSave()
    {
        UpdateSaveStatus("Quick saving...", 0f);
        _coordinator.QuickSave();
    }

    private void OnSaveNameChanged(string newText)
    {
        _saveButton.interactable = !string.IsNullOrEmpty(newText);
    }

    public void QuickLoad()
    {
        UpdateLoadStatus("Quick loading...", 0f);
        _coordinator.QuickLoad();
    }

    public void RefreshSaveFilesList()
    {
        ClearSaveFilesList();

        var saveFiles = _coordinator.GetSaveFiles();
        if (saveFiles.Count == 0)
        {
            UpdateLoadStatus("No save files found", 3f);
            return;
        }

        foreach (var saveFile in saveFiles)
        {
            CreateSaveFileEntry(saveFile);
        }

        UpdateLoadStatus($"Found {saveFiles.Count} save files", 3f);
    }

    private void CreateSaveFileEntry(string saveName)
    {
        if (_saveFileEntryPrefab == null) return;

        var entryUI = Instantiate(_saveFileEntryPrefab, _saveFilesContainer);
        if (entryUI != null)
        {
            entryUI.Initialize(saveName, OnLoadFile, OnDeleteFile);
            _saveFileEntries.Add(entryUI);
        }
    }

    private void ClearSaveFilesList()
    {
        foreach (var entry in _saveFileEntries)
        {
            if (entry != null) Destroy(entry.gameObject);
        }
        _saveFileEntries.Clear();
    }

    private void OnLoadFile(string saveName)
    {
        UpdateLoadStatus($"Loading {saveName}...", 0f);
        _coordinator.LoadGraph(saveName);
    }

    private void OnDeleteFile(string saveName)
    {
        _coordinator.DeleteSaveFile(saveName);
        RefreshSaveFilesList();
        UpdateManagementStatus($"Deleted {saveName}", 3f);
    }

    public void DeleteAllSaves()
    {
        _coordinator.DeleteAllSaves();
        RefreshSaveFilesList();
        UpdateManagementStatus("All saves deleted", 3f);
    }

    private void OnGraphSaved(string saveName)
    {
        UpdateSaveStatus($"Saved: {saveName}", 3f);
        RefreshSaveFilesList();
    }

    private void OnGraphLoaded(string saveName)
    {
        UpdateLoadStatus($"Loaded: {saveName}", 3f);
    }

    private void UpdateSaveStatus(string message, float displayTime = 0f)
    {
        if (_saveStatusText != null)
        {
            _saveStatusText.text = message;
            if (displayTime > 0) _statusDisplayTimer = displayTime;
        }
    }

    private void UpdateLoadStatus(string message, float displayTime = 0f)
    {
        if (_loadStatusText != null)
        {
            _loadStatusText.text = message;
            if (displayTime > 0) _statusDisplayTimer = displayTime;
        }
    }

    private void UpdateManagementStatus(string message, float displayTime = 0f)
    {
        if (_managementStatusText != null)
        {
            _managementStatusText.text = message;
            if (displayTime > 0) _statusDisplayTimer = displayTime;
        }
    }

    private void UpdateStatusTimers()
    {
        if (_statusDisplayTimer > 0)
        {
            _statusDisplayTimer -= Time.deltaTime;
            if (_statusDisplayTimer <= 0)
            {
                UpdateSaveStatus("Ready to save");
                UpdateLoadStatus($"{GetSaveFilesCount()} save files");
            }
        }
    }

    private int GetSaveFilesCount()
    {
        if (_coordinator == null) return 0;
        return _coordinator.GetSaveFiles().Count;
    }
}