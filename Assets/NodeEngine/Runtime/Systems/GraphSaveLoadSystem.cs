using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles saving and loading of node graphs to/from persistent storage.
/// Works with the existing NodeSpawnerService, ConnectionManager, and LineRenderersController.
/// </summary>
public class GraphSaveLoadSystem : MonoBehaviour
{
    public static GraphSaveLoadSystem Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string _saveFileExtension = ".json";
    [SerializeField] private string _quickSaveSlotName = "QuickSave";
    [SerializeField] private bool _prettyPrint = true;

    [Header("Dependencies")]
    [SerializeField] private NodesDatabase _nodesDatabase; // Assign in Inspector to restore node icons

    // Events for UI feedback
    public event Action<string> OnGraphSaved;
    public event Action<string> OnGraphLoaded;
    public event Action<string> OnSaveDeleted;

    private string SaveDirectory => Application.persistentDataPath + "/NodeGraphs/";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure save directory exists
        if (!Directory.Exists(SaveDirectory))
            Directory.CreateDirectory(SaveDirectory);
    }

    #region Public API

    /// <summary>
    /// Save the current graph with a custom name.
    /// </summary>
    public void SaveGraph(string saveName)
    {
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogWarning("[GraphSaveLoadSystem] Cannot save with empty name.");
            return;
        }

        try
        {
            var graphData = CaptureCurrentGraph();
            string json = JsonUtility.ToJson(graphData, _prettyPrint);
            string filePath = GetFilePath(saveName);

            File.WriteAllText(filePath, json);
            Debug.Log($"[GraphSaveLoadSystem] Graph saved to: {filePath}");

            OnGraphSaved?.Invoke(saveName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GraphSaveLoadSystem] Failed to save graph '{saveName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Load a graph by name, replacing the current graph.
    /// </summary>
    public void LoadGraph(string saveName)
    {
        string filePath = GetFilePath(saveName);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[GraphSaveLoadSystem] Save file not found: {saveName}");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var graphData = JsonUtility.FromJson<SerializableGraph>(json);

            ClearCurrentGraph();
            RestoreGraph(graphData);

            Debug.Log($"[GraphSaveLoadSystem] Graph loaded from: {saveName}");
            OnGraphLoaded?.Invoke(saveName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GraphSaveLoadSystem] Failed to load graph '{saveName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Quick save to a predefined slot.
    /// </summary>
    public void QuickSave()
    {
        SaveGraph(_quickSaveSlotName);
    }

    /// <summary>
    /// Quick load from the predefined slot.
    /// </summary>
    public void QuickLoad()
    {
        LoadGraph(_quickSaveSlotName);
    }

    /// <summary>
    /// Get a list of all available save file names (without extension).
    /// </summary>
    public List<string> GetSaveFiles()
    {
        var files = new List<string>();
        if (!Directory.Exists(SaveDirectory))
            return files;

        foreach (string file in Directory.GetFiles(SaveDirectory, "*" + _saveFileExtension))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            files.Add(fileName);
        }
        return files.OrderBy(f => f).ToList();
    }

    /// <summary>
    /// Delete a specific save file.
    /// </summary>
    public void DeleteSaveFile(string saveName)
    {
        string filePath = GetFilePath(saveName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[GraphSaveLoadSystem] Deleted save: {saveName}");
            OnSaveDeleted?.Invoke(saveName);
        }
    }

    /// <summary>
    /// Delete all save files.
    /// </summary>
    public void DeleteAllSaves()
    {
        foreach (string file in Directory.GetFiles(SaveDirectory, "*" + _saveFileExtension))
        {
            File.Delete(file);
        }
        Debug.Log("[GraphSaveLoadSystem] Deleted all saves.");
        OnSaveDeleted?.Invoke(null);
    }

    #endregion

    #region Graph Capture & Restoration

    private SerializableGraph CaptureCurrentGraph()
    {
        var graph = new SerializableGraph();
        graph.version = "1.0";

        // Capture nodes
        foreach (var nodeLogic in NodeSpawnerService.Instance.GetAllNodes())
        {
            var node = nodeLogic.Node;
            var nodeData = new SerializableGraph.NodeData
            {
                nodeId = node.NodeId,
                nodeType = node.GetType().AssemblyQualifiedName,
                position = new SerializableVector2(nodeLogic.transform.localPosition),
                nodeName = node.NodeName
            };

            // Capture variable node values
            if (node is VariableNode varNode)
            {
                nodeData.variableType = (int)varNode.VariableType;
                nodeData.serializedValue = SerializeVariableValue(varNode);
            }

            graph.nodes.Add(nodeData);
        }

        // Capture data connections
        foreach (var conn in ConnectionManager.Instance.ActiveDataConnections)
        {
            graph.connections.Add(new SerializableGraph.ConnectionData
            {
                sourceNodeId = conn.SourceNode.NodeId,
                sourcePortName = conn.OutputPortName,
                targetNodeId = conn.TargetNode.NodeId,
                targetPortName = conn.InputPortName,
                isFlow = false
            });
        }

        // Capture flow connections
        foreach (var conn in ConnectionManager.Instance.ActiveFlowConnections)
        {
            graph.connections.Add(new SerializableGraph.ConnectionData
            {
                sourceNodeId = conn.SourceNode.NodeId,
                sourcePortName = conn.SourcePortName,
                targetNodeId = conn.TargetNode.NodeId,
                targetPortName = conn.TargetPortName,
                isFlow = true
            });
        }

        return graph;
    }

    private void ClearCurrentGraph()
    {
        // Delete all visual nodes (this also clears their connections)
        var nodes = NodeSpawnerService.Instance.GetAllNodes().ToList();
        foreach (var nodeLogic in nodes)
        {
            NodeSpawnerService.Instance.DeleteNode(nodeLogic);
        }

        // Clear any remaining logical connections
        ConnectionManager.Instance.ActiveDataConnections.Clear();
        ConnectionManager.Instance.ActiveFlowConnections.Clear();

        // Clear all visual lines
        LineRenderersController.ClearAllConnections();
    }

    private void RestoreGraph(SerializableGraph graph)
    {
        var nodeLookup = new Dictionary<string, BaseNode>();

        // First pass: instantiate all nodes
        foreach (var nodeData in graph.nodes)
        {
            Type nodeType = Type.GetType(nodeData.nodeType);
            if (nodeType == null)
            {
                Debug.LogError($"[GraphSaveLoadSystem] Could not resolve type: {nodeData.nodeType}");
                continue;
            }

            BaseNode nodeInstance;
            try
            {
                nodeInstance = (BaseNode)Activator.CreateInstance(nodeType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GraphSaveLoadSystem] Failed to create node of type {nodeType.Name}: {ex.Message}");
                continue;
            }

            // Apply node name and icon from database
            ApplyNodeMetadata(nodeInstance, nodeType, nodeData.nodeName);

            // Restore variable value if applicable
            if (nodeInstance is VariableNode varNode && !string.IsNullOrEmpty(nodeData.serializedValue))
            {
                DeserializeVariableValue(varNode, nodeData.serializedValue);
            }

            // Spawn the visual representation
            Vector2 pos = new Vector2(nodeData.position.x, nodeData.position.y);
            var nodeLogic = NodeSpawnerService.Instance.SpawnNode(nodeInstance, pos, nodeData.nodeId);

            if (nodeLogic != null)
                nodeLookup[nodeData.nodeId] = nodeInstance;
        }

        // Second pass: restore connections and create visual lines
        foreach (var connData in graph.connections)
        {
            if (!nodeLookup.TryGetValue(connData.sourceNodeId, out BaseNode sourceNode) ||
                !nodeLookup.TryGetValue(connData.targetNodeId, out BaseNode targetNode))
            {
                Debug.LogWarning($"[GraphSaveLoadSystem] Skipping connection: source or target node missing.");
                continue;
            }

            // Find the connectors
            Connector sourceConn = FindConnector(sourceNode, connData.sourcePortName, isOutput: true);
            Connector targetConn = FindConnector(targetNode, connData.targetPortName, isOutput: false);

            if (sourceConn == null)
            {
                Debug.LogWarning($"[GraphSaveLoadSystem] Source connector not found: {sourceNode.GetType().Name}.{connData.sourcePortName}");
                continue;
            }
            if (targetConn == null)
            {
                Debug.LogWarning($"[GraphSaveLoadSystem] Target connector not found: {targetNode.GetType().Name}.{connData.targetPortName}");
                continue;
            }

            // Create the logical connection
            bool success = ConnectionManager.Instance.CreateConnectionWithConnectors(sourceConn, targetConn);
            if (success)
            {
                // Create visual line
                CreateVisualConnectionLine(sourceConn, targetConn);
            }
        }

        // Mark graph dirty to recompile
        NodeRunner.Instance?.MarkDirty();

        Debug.Log($"[GraphSaveLoadSystem] Restored {graph.nodes.Count} nodes and {graph.connections.Count} connections.");
    }

    /// <summary>
    /// Sets the node's Name and Sprite from the NodesDatabase.
    /// </summary>
    private void ApplyNodeMetadata(BaseNode node, Type nodeType, string savedName)
    {
        string finalName = savedName;
        Sprite icon = null;

        // FIX: If database is null (e.g. system created dynamically), try to find it in loaded resources
        if (_nodesDatabase == null)
        {
            _nodesDatabase = Resources.FindObjectsOfTypeAll<NodesDatabase>().FirstOrDefault();
        }

        if (_nodesDatabase != null)
        {
            var templates = _nodesDatabase.GetNodes();
            // FIX: Added null check to prevent NullReferenceExceptions during type evaluation
            var template = templates.FirstOrDefault(n => n != null && n.GetType() == nodeType);
            if (template != null)
            {
                if (string.IsNullOrEmpty(finalName))
                    finalName = template.NodeName;
                icon = template.NodeSprite;
            }
        }

        if (string.IsNullOrEmpty(finalName))
            finalName = nodeType.Name.Replace("Node", "");

        node.SetName(finalName);
        node.SetIcon(icon);
    }

    /// <summary>
    /// Finds a connector by port name. For outputs, search OutputConnectors; for inputs, search InputConnectors.
    /// </summary>
    private Connector FindConnector(BaseNode node, string portName, bool isOutput)
    {
        var logic = node.LogicView;
        if (logic == null) return null;

        if (isOutput)
            return logic.OutputConnectors.FirstOrDefault(c => c.PortName == portName);
        else
            return logic.InputConnectors.FirstOrDefault(c => c.PortName == portName);
    }

    /// <summary>
    /// Instantiates and registers a visual line between two connectors using LineRenderersController.
    /// </summary>
    private void CreateVisualConnectionLine(Connector a, Connector b)
    {
        if (LineRenderersController.Instance == null)
        {
            Debug.LogError("[GraphSaveLoadSystem] LineRenderersController.Instance is null!");
            return;
        }

        var linePrefab = LineRenderersController.Instance.LineRendererPrefab;
        if (linePrefab == null)
        {
            Debug.LogError("[GraphSaveLoadSystem] LineRendererPrefab not assigned in LineRenderersController!");
            return;
        }

        // Instantiate the line prefab (parent will be set by LineRenderersController.Add)
        var lineInstance = Instantiate(linePrefab);

        // Register with the controller – it handles parenting, material, and updates.
        LineRenderersController.Add(a, b, lineInstance);
    }

    #endregion

    #region Variable Serialization Helpers

    private string SerializeVariableValue(VariableNode varNode)
    {
        object value = varNode.GetValue();
        if (value == null) return "";

        return varNode.VariableType switch
        {
            VariableType.Vector3 => JsonUtility.ToJson((Vector3)value),
            VariableType.Vector2 => JsonUtility.ToJson((Vector2)value),
            VariableType.Color => JsonUtility.ToJson((Color)value),
            VariableType.Type => ((Type)value).AssemblyQualifiedName,
            _ => value.ToString()
        };
    }

    private void DeserializeVariableValue(VariableNode varNode, string serialized)
    {
        if (string.IsNullOrEmpty(serialized)) return;

        try
        {
            object value = varNode.VariableType switch
            {
                VariableType.Single => float.Parse(serialized, System.Globalization.CultureInfo.InvariantCulture),
                VariableType.Int => int.Parse(serialized),
                VariableType.Bool => bool.Parse(serialized),
                VariableType.String => serialized,
                VariableType.Vector3 => JsonUtility.FromJson<Vector3>(serialized),
                VariableType.Vector2 => JsonUtility.FromJson<Vector2>(serialized),
                VariableType.Color => JsonUtility.FromJson<Color>(serialized),
                VariableType.Type => Type.GetType(serialized) ?? typeof(object),
                _ => null
            };
            varNode.SetValue(value);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GraphSaveLoadSystem] Failed to deserialize {varNode.VariableType}: {ex.Message}");
        }
    }

    #endregion

    #region Helpers

    private string GetFilePath(string saveName)
    {
        // Sanitize filename
        foreach (char c in Path.GetInvalidFileNameChars())
            saveName = saveName.Replace(c, '_');
        return Path.Combine(SaveDirectory, saveName + _saveFileExtension);
    }

    #endregion

    #region Serializable Data Structures

    [Serializable]
    public class SerializableGraph
    {
        public string version;
        public List<NodeData> nodes = new List<NodeData>();
        public List<ConnectionData> connections = new List<ConnectionData>();

        [Serializable]
        public class NodeData
        {
            public string nodeId;
            public string nodeType;
            public string nodeName;
            public SerializableVector2 position;
            public int variableType;
            public string serializedValue;
        }

        [Serializable]
        public class ConnectionData
        {
            public string sourceNodeId;
            public string sourcePortName;
            public string targetNodeId;
            public string targetPortName;
            public bool isFlow;
        }
    }

    [Serializable]
    public struct SerializableVector2
    {
        public float x, y;
        public SerializableVector2(Vector2 v) { x = v.x; y = v.y; }
        public Vector2 ToVector2() => new Vector2(x, y);
    }

    #endregion
}