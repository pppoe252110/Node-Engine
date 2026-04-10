using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class GraphSaveLoadCoordinator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string _quickSaveSlotName = "QuickSave";
    [SerializeField] private bool _prettyPrint = true;
    [SerializeField] private NodesDatabase _nodesDatabase;

    private INodeFactory _nodeFactory;
    private IGraphStorage _storage;
    private GraphSerializer _serializer;
    private NodeSpawnerService _nodeSpawner;
    private ConnectionManager _connectionManager;
    private NodeRunner _nodeRunner;
    private LineRenderersController _lineRenderersController;

    public event Action<string> OnGraphSaved;
    public event Action<string> OnGraphLoaded;
    public event Action<string> OnSaveDeleted;

    [Inject]
    public void Construct(
        INodeFactory nodeFactory,
        IGraphStorage storage,
        GraphSerializer serializer,
        NodeSpawnerService nodeSpawner,
        ConnectionManager connectionManager,
        NodeRunner nodeRunner,
        LineRenderersController lineRenderersController)
    {
        _nodeFactory = nodeFactory;
        _storage = storage;
        _serializer = serializer;
        _nodeSpawner = nodeSpawner;
        _connectionManager = connectionManager;
        _nodeRunner = nodeRunner;
        _lineRenderersController = lineRenderersController;
    }

    public void SaveGraph(string saveName)
    {
        if (string.IsNullOrEmpty(saveName)) return;

        try
        {
            var graphData = _serializer.Serialize(
                _nodeSpawner.GetAllNodes(),
                _connectionManager.ActiveDataConnections,
                _connectionManager.ActiveFlowConnections
            );

            string json = _serializer.SerializeToJson(graphData, _prettyPrint);
            _storage.Save(saveName, json);

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
            string json = _storage.Load(saveName);
            var graphData = _serializer.DeserializeFromJson(json);

            ClearCurrentGraph();
            RestoreGraph(graphData);

            OnGraphLoaded?.Invoke(saveName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GraphCoordinator] Failed to load graph '{saveName}': {ex.Message} \n {ex.InnerException}");
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
        // Deleting nodes will disconnect them, which fires events that clean up lines.
        var nodes = _nodeSpawner.GetAllNodes().ToList();
        foreach (var nodeLogic in nodes)
        {
            _nodeSpawner.DeleteNode(nodeLogic);
        }

        // Ensure all connection lists are clear (should already be empty after node deletions).
        _connectionManager.ClearAllConnections();
    }

    private void RestoreGraph(SerializableGraph graph)
    {
        Debug.Log("[RestoreGraph] Starting restore...");

        var nodeLookup = new Dictionary<string, BaseNode>();

        foreach (var nodeData in graph.nodes)
        {
            Type nodeType = Type.GetType(nodeData.nodeType);
            if (nodeType == null)
            {
                Debug.LogWarning($"Type not found: {nodeData.nodeType}");
                continue;
            }

            BaseNode nodeInstance;
            try
            {
                nodeInstance = _nodeFactory.CreateNode(nodeType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create instance: {ex.Message}");
                continue;
            }

            ApplyNodeMetadata(nodeInstance, nodeType, nodeData.nodeName);

            if (nodeInstance is VariableNode varNode && !string.IsNullOrEmpty(nodeData.serializedValue))
            {
                _serializer.DeserializeVariableValue(varNode, nodeData.serializedValue);
            }

            Vector2 pos = new Vector2(nodeData.position.x, nodeData.position.y);
            var nodeLogic = _nodeSpawner.SpawnNode(nodeInstance, pos, nodeData.nodeId);

            if (nodeLogic != null)
                nodeLookup[nodeData.nodeId] = nodeInstance;
            else
                Debug.LogError($"[RestoreGraph] SpawnNode returned null for {nodeData.nodeId}");
        }

        foreach (var connData in graph.connections)
        {
            if (!nodeLookup.TryGetValue(connData.sourceNodeId, out BaseNode sourceNode) ||
                !nodeLookup.TryGetValue(connData.targetNodeId, out BaseNode targetNode))
            {
                Debug.LogWarning("Source or target node not found in lookup.");
                continue;
            }

            Connector sourceConn = FindConnector(sourceNode, connData.sourcePortName, isOutput: true);
            Connector targetConn = FindConnector(targetNode, connData.targetPortName, isOutput: false);

            if (sourceConn == null || targetConn == null)
            {
                Debug.LogWarning($"Connector not found: {connData.sourcePortName} or {connData.targetPortName}");
                continue;
            }

            // ConnectionManager will fire OnConnectionAdded, which LineRenderersController handles.
            _connectionManager.CreateConnectionWithConnectors(sourceConn, targetConn);
        }

        _nodeRunner?.MarkDirty();
        Debug.Log("[RestoreGraph] Restore complete.");
    }

    private void ApplyNodeMetadata(BaseNode node, Type nodeType, string savedName)
    {
        _nodesDatabase?.ApplyMetadata(node);

        if (!string.IsNullOrEmpty(savedName))
        {
            node.SetName(savedName);
        }
        else if (string.IsNullOrEmpty(node.NodeName))
        {
            node.SetName(nodeType.Name.Replace("Node", ""));
        }
    }

    private Connector FindConnector(BaseNode node, string portName, bool isOutput)
    {
        var logic = node.LogicView;
        if (logic == null) return null;

        return isOutput
            ? logic.OutputConnectors.FirstOrDefault(c => c.PortName == portName)
            : logic.InputConnectors.FirstOrDefault(c => c.PortName == portName);
    }
}