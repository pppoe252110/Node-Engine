using NodeEngine.GraphPersistence;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class GraphSaveLoadCoordinator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string _quickSaveSlotName = "QuickSave";

    private GraphSaveService _saveService;
    private GraphLoadService _loadService;
    private GraphSnapshotBuilder _snapshotBuilder;
    private GraphRestorer _restorer;
    private IGraphStorage _storage;
    private NodeSpawnerService _nodeSpawner;
    private ConnectionService _connectionService;

    public event Action<string> OnGraphSaved;
    public event Action<string> OnGraphLoaded;
    public event Action<string> OnSaveDeleted;

    [Inject]
    public void Construct(
        GraphSaveService saveService,
        GraphLoadService loadService,
        GraphSnapshotBuilder snapshotBuilder,
        GraphRestorer restorer,
        IGraphStorage storage,
        NodeSpawnerService nodeSpawner,
        ConnectionService connectionService)
    {
        _saveService = saveService;
        _loadService = loadService;
        _snapshotBuilder = snapshotBuilder;
        _restorer = restorer;
        _storage = storage;
        _nodeSpawner = nodeSpawner;
        _connectionService = connectionService;
    }

    public void SaveGraph(string saveName)
    {
        if (string.IsNullOrEmpty(saveName)) return;

        try
        {
            var snapshot = _snapshotBuilder.BuildSnapshot();
            _saveService.Save(saveName, snapshot);
            OnGraphSaved?.Invoke(saveName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GraphCoordinator] Failed to save graph '{saveName}': {ex.Message}");
        }
    }

    public void LoadGraph(string saveName)
    {
        if (!_storage.Exists(saveName)) return;

        try
        {
            var snapshot = _loadService.Load(saveName);
            ClearCurrentGraph();
            _restorer.Restore(snapshot);
            OnGraphLoaded?.Invoke(saveName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GraphCoordinator] Failed to load graph '{saveName}': {ex.Message}");
        }
    }

    public void QuickSave() => SaveGraph(_quickSaveSlotName);
    public void QuickLoad() => LoadGraph(_quickSaveSlotName);
    public List<string> GetSaveFiles() => _storage.GetAllSaveNames();

    public void DeleteSaveFile(string saveName)
    {
        _storage.Delete(saveName);
        OnSaveDeleted?.Invoke(saveName);
    }

    public void DeleteAllSaves()
    {
        _storage.DeleteAll();
        OnSaveDeleted?.Invoke(null);
    }

    private void ClearCurrentGraph()
    {
        NodeEngine.Core.NodeEngine.SetIsClearingGraph(true);
        try
        {
            var nodes = _nodeSpawner.GetAllNodes().ToList();
            foreach (var nodeLogic in nodes)
                _nodeSpawner.DeleteNode(nodeLogic);
            _connectionService.ClearAllConnections();
        }
        finally
        {
            NodeEngine.Core.NodeEngine.SetIsClearingGraph(false);
        }
    }
}