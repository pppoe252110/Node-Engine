using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.InputSystem;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject _saveLoadPanel;
    [SerializeField] private Button _openButton;

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

    private void Start()
    {
        SetupUI();
        RefreshSaveFilesList();

        GraphSaveLoadSystem.Instance.OnGraphSaved += OnGraphSaved;
        GraphSaveLoadSystem.Instance.OnGraphLoaded += OnGraphLoaded;
    }

    private void OnDestroy()
    {
        if (GraphSaveLoadSystem.Instance != null)
        {
            GraphSaveLoadSystem.Instance.OnGraphSaved -= OnGraphSaved;
            GraphSaveLoadSystem.Instance.OnGraphLoaded -= OnGraphLoaded;
        }
    }

    private void Update()
    {
        HandleHotkeys();
        UpdateStatusTimers();
    }

    private void SetupUI()
    {
        _openButton.onClick.AddListener(TogglePanel);

        _saveButton.onClick.AddListener(SaveGraph);
        _quickSaveButton.onClick.AddListener(QuickSave);
        _saveNameInput.onSubmit.AddListener((text) => SaveGraph());
        _saveNameInput.onValueChanged.AddListener(OnSaveNameChanged);

        _refreshButton.onClick.AddListener(RefreshSaveFilesList);
        _quickLoadButton.onClick.AddListener(QuickLoad);

        _deleteAllSavesButton.onClick.AddListener(DeleteAllSaves);

        UpdateSaveStatus("Ready to save");
        UpdateLoadStatus($"{GetSaveFilesCount()} save files");

        ClosePanel();
    }

    private void HandleHotkeys()
    {
        if (Keyboard.current.f5Key.wasReleasedThisFrame) QuickSave();
        else if (Keyboard.current.f9Key.wasReleasedThisFrame) QuickLoad();
        else if (Keyboard.current.escapeKey.wasReleasedThisFrame && _saveLoadPanel.activeInHierarchy) ClosePanel();
    }

    public void SaveGraph()
    {
        if (string.IsNullOrEmpty(_saveNameInput.text))
        {
            UpdateSaveStatus("Please enter a save name", 3f);
            return;
        }

        UpdateSaveStatus("Saving...", 0f);
        GraphSaveLoadSystem.Instance.SaveGraph(_saveNameInput.text);
    }

    public void QuickSave()
    {
        UpdateSaveStatus("Quick saving...", 0f);
        GraphSaveLoadSystem.Instance.QuickSave();
    }

    private void OnSaveNameChanged(string newText)
    {
        _saveButton.interactable = !string.IsNullOrEmpty(newText);
    }

    public void QuickLoad()
    {
        UpdateLoadStatus("Quick loading...", 0f);
        GraphSaveLoadSystem.Instance.QuickLoad();
    }

    public void RefreshSaveFilesList()
    {
        ClearSaveFilesList();

        var saveFiles = GraphSaveLoadSystem.Instance.GetSaveFiles();
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
        GraphSaveLoadSystem.Instance.LoadGraph(saveName);
    }

    private void OnDeleteFile(string saveName)
    {
        GraphSaveLoadSystem.Instance.DeleteSaveFile(saveName);
        RefreshSaveFilesList();
        UpdateManagementStatus($"Deleted {saveName}", 3f);
    }

    public void DeleteAllSaves()
    {
        GraphSaveLoadSystem.Instance.DeleteAllSaves();
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

    public void TogglePanel()
    {
        _saveLoadPanel.SetActive(!_saveLoadPanel.activeSelf);
        RefreshSaveFilesList();
    }

    public void ClosePanel() => _saveLoadPanel.SetActive(false);

    private void UpdateSaveStatus(string message, float displayTime = 0f)
    {
        _saveStatusText.text = message;
        if (displayTime > 0) _statusDisplayTimer = displayTime;
    }

    private void UpdateLoadStatus(string message, float displayTime = 0f)
    {
        _loadStatusText.text = message;
        if (displayTime > 0) _statusDisplayTimer = displayTime;
    }

    private void UpdateManagementStatus(string message, float displayTime = 0f)
    {
        _managementStatusText.text = message;
        if (displayTime > 0) _statusDisplayTimer = displayTime;
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

    private int GetSaveFilesCount() => GraphSaveLoadSystem.Instance.GetSaveFiles().Count;
}