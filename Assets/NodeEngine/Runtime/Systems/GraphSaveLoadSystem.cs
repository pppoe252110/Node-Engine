using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class GraphSaveData
{
    public List<NodeInstanceData> nodes = new List<NodeInstanceData>();
    public List<ConnectionSaveData> connections = new List<ConnectionSaveData>();
    public string saveTime;
    public int version = 2;
}

[Serializable]
public class NodeInstanceData
{
    public int instanceId;
    public string databaseId;
    public Vector2 position;
    public string fieldValuesJson;
    public string nodeType;
    public VariableType variableType;
}

[Serializable]
public class ConnectionSaveData
{
    public int fromNodeId;
    public int toNodeId;
    public string fromConnectorName;
    public string toConnectorName;
}

[Serializable]
public class FieldValueData
{
    public string attributeName;
    public string value;
}

[Serializable]
public class FieldValueDataList
{
    public List<FieldValueData> values;
}

public class GraphSaveLoadSystem : MonoBehaviour
{
    public static GraphSaveLoadSystem Instance { get; private set; }

    [SerializeField] private NodesDatabase _nodesDatabase;

    public event Action<string> OnGraphSaved;
    public event Action<string> OnGraphLoaded;

    private List<IValuePersister> _valuePersisters = new List<IValuePersister>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        InitializeValuePersisters();
    }

    private void InitializeValuePersisters()
    {
        _valuePersisters = new List<IValuePersister>
        {
            new TypeValuePersister(),
            new BasicValuePersister(),
            new Vector3ValuePersister()
        };
    }

    public void SaveGraph(string saveName)
    {
        if (string.IsNullOrEmpty(saveName)) return;

        try
        {
            var saveData = CreateSaveData();
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = GetSavePath(saveName);
            File.WriteAllText(filePath, json);
            Debug.Log($"SAVED: {saveData.nodes.Count} nodes, {saveData.connections.Count} connections");
            OnGraphSaved?.Invoke(saveName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    public void LoadGraph(string saveName)
    {
        if (string.IsNullOrEmpty(saveName)) return;

        try
        {
            string filePath = GetSavePath(saveName);
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return;
            }

            string json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<GraphSaveData>(json);
            Debug.Log($"LOADING: {saveData.nodes.Count} nodes, {saveData.connections.Count} connections");
            LoadFromSaveData(saveData);
            OnGraphLoaded?.Invoke(saveName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
        }
    }

    private GraphSaveData CreateSaveData()
    {
        var saveData = new GraphSaveData
        {
            saveTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            version = 2
        };

        foreach (var (nodeId, nodeLogic) in NodeSpawnerService.Instance.GetAllNodes())
        {
            if (nodeLogic == null || nodeLogic.Node == null) continue;

            var databaseId = FindDatabaseIdForNode(nodeLogic.Node);
            var nodeSaveData = new NodeInstanceData
            {
                instanceId = nodeLogic.Node.Guid,
                databaseId = databaseId,
                position = nodeLogic.transform.localPosition,
                variableType = nodeLogic.Node is VariableNode varNode ? varNode.VariableType : VariableType.Object
            };
            SaveNodeData(nodeLogic.Node, nodeSaveData);
            saveData.nodes.Add(nodeSaveData);
        }

        SaveAllConnections(saveData);
        return saveData;
    }

    private void SaveAllConnections(GraphSaveData saveData)
    {
        var savedConnections = new HashSet<string>();

        SaveConnectorConnections(saveData, savedConnections);
        SaveVisualConnections(saveData, savedConnections);

        Debug.Log($"CONNECTIONS SAVED: {saveData.connections.Count}");
    }

    private void SaveConnectorConnections(GraphSaveData saveData, HashSet<string> savedConnections)
    {
        foreach (var (nodeId, nodeLogic) in NodeSpawnerService.Instance.GetAllNodes())
        {
            if (nodeLogic?.Node == null) continue;

            foreach (var outputConnector in nodeLogic.Node.outputConnectors)
            {
                foreach (var connectedConnector in outputConnector.Connections)
                {
                    if (connectedConnector?.Node == null) continue;

                    var fromAttr = outputConnector.Field?.GetAttribute();
                    var toAttr = connectedConnector.Field?.GetAttribute();

                    if (fromAttr != null && toAttr != null)
                    {
                        SaveConnection(saveData, savedConnections,
                            nodeLogic.Node.Guid, connectedConnector.Node.Guid,
                            fromAttr.attributeName, toAttr.attributeName);
                    }
                }
            }
        }
    }

    private void SaveVisualConnections(GraphSaveData saveData, HashSet<string> savedConnections)
    {
        if (LineRenderersController.Instance == null) return;

        try
        {
            var controllerType = typeof(LineRenderersController);
            var connectionsField = controllerType.GetField("_connections",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (connectionsField != null)
            {
                var connections = connectionsField.GetValue(LineRenderersController.Instance) as List<LineRenderersController.ConnectionData>;
                if (connections != null)
                {
                    foreach (var connection in connections)
                    {
                        if (connection.IsValid && connection.ConnectorA != null && connection.ConnectorB != null)
                        {
                            var fromAttr = connection.ConnectorA.Field?.GetAttribute();
                            var toAttr = connection.ConnectorB.Field?.GetAttribute();

                            if (fromAttr != null && toAttr != null)
                            {
                                SaveConnection(saveData, savedConnections,
                                    connection.ConnectorA.Node.Guid, connection.ConnectorB.Node.Guid,
                                    fromAttr.attributeName, toAttr.attributeName);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Visual connections save failed: {e.Message}");
        }
    }

    private void SaveConnection(GraphSaveData saveData, HashSet<string> savedConnections,
        int fromNodeId, int toNodeId, string fromConnectorName, string toConnectorName)
    {
        if (fromNodeId == toNodeId) return;
        if (string.IsNullOrEmpty(fromConnectorName) || fromConnectorName == "Unknown") return;
        if (string.IsNullOrEmpty(toConnectorName) || toConnectorName == "Unknown") return;

        string key = $"{fromNodeId}_{toNodeId}_{fromConnectorName}_{toConnectorName}";
        if (!savedConnections.Contains(key))
        {
            saveData.connections.Add(new ConnectionSaveData
            {
                fromNodeId = fromNodeId,
                toNodeId = toNodeId,
                fromConnectorName = fromConnectorName,
                toConnectorName = toConnectorName
            });
            savedConnections.Add(key);
        }
    }

    private string FindDatabaseIdForNode(NodeBase node)
    {
        var nodes = _nodesDatabase.GetNodes();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].GetType() == node.GetType() && nodes[i].NodeName == node.NodeName)
            {
                return $"{node.GetType().Name}_{node.NodeName}";
            }
        }
        return "unknown";
    }

    private void LoadFromSaveData(GraphSaveData saveData)
    {
        ClearCurrentGraph();

        // PHASE 1: Spawn all nodes
        var loadedNodes = SpawnAllNodes(saveData);

        // Wait one frame for UI to initialize
        StartCoroutine(LoadPhase2(saveData, loadedNodes));
    }

    private Dictionary<int, (NodeLogic nodeLogic, object valueToLoad)> SpawnAllNodes(GraphSaveData saveData)
    {
        var loadedNodes = new Dictionary<int, (NodeLogic, object)>();
        var databaseNodes = _nodesDatabase.GetNodes().ToDictionary(
            n => $"{n.GetType().Name}_{n.NodeName}",
            n => n
        );

        foreach (var nodeSaveData in saveData.nodes)
        {
            if (databaseNodes.TryGetValue(nodeSaveData.databaseId, out var databaseNode))
            {
                // Spawn the node
                var nodeLogic = NodeSpawnerService.Instance.SpawnNode(
                    _nodesDatabase.GetClone(databaseNode),
                    nodeSaveData.position,
                    nodeSaveData.instanceId
                );

                if (nodeLogic != null)
                {
                    // Parse the value to load (but don't load it yet)
                    object valueToLoad = ParseNodeValue(nodeSaveData);
                    loadedNodes[nodeSaveData.instanceId] = (nodeLogic, valueToLoad);
                }
            }
        }

        return loadedNodes;
    }

    private System.Collections.IEnumerator LoadPhase2(GraphSaveData saveData, Dictionary<int, (NodeLogic nodeLogic, object valueToLoad)> loadedNodes)
    {
        // Wait for UI to be fully initialized
        yield return null;

        // PHASE 2: Connect all nodes
        ConnectNodes(saveData, loadedNodes.Keys.ToHashSet());

        // Wait one more frame for connections to be established
        yield return null;

        // PHASE 3: Load all values (UI is now ready)
        LoadAllValues(loadedNodes);

        Debug.Log("Graph loading complete!");
    }

    private void ConnectNodes(GraphSaveData saveData, HashSet<int> loadedNodeIds)
    {
        foreach (var connection in saveData.connections)
        {
            if (IsValidConnectionForLoading(connection, loadedNodeIds))
            {
                if (!ConnectNodesByAttributeNames(connection.fromNodeId, connection.toNodeId,
                    connection.fromConnectorName, connection.toConnectorName))
                {
                    Debug.LogWarning($"Can't connect {connection.fromConnectorName} to {connection.toConnectorName}");
                }
            }
        }
    }

    private bool IsValidConnectionForLoading(ConnectionSaveData connection, HashSet<int> loadedNodeIds)
    {
        if (string.IsNullOrEmpty(connection.fromConnectorName) || connection.fromConnectorName == "Unknown" ||
            string.IsNullOrEmpty(connection.toConnectorName) || connection.toConnectorName == "Unknown" ||
            connection.fromNodeId == connection.toNodeId)
        {
            return false;
        }

        if (!loadedNodeIds.Contains(connection.fromNodeId) || !loadedNodeIds.Contains(connection.toNodeId))
        {
            return false;
        }

        return true;
    }

    private bool ConnectNodesByAttributeNames(int fromNodeId, int toNodeId, string fromConnectorName, string toConnectorName)
    {
        var fromNode = NodeSpawnerService.Instance.GetNodeById(fromNodeId);
        var toNode = NodeSpawnerService.Instance.GetNodeById(toNodeId);

        if (fromNode == null || toNode == null)
        {
            Debug.LogWarning($"Nodes not found: {fromNodeId} -> {toNodeId}");
            return false;
        }

        Connector fromConnector = FindConnectorByAttributeName(fromNode.Node.outputConnectors, fromConnectorName);
        Connector toConnector = FindConnectorByAttributeName(toNode.Node.inputConnectors, toConnectorName);

        if (fromConnector == null || toConnector == null)
        {
            Debug.LogWarning($"Connectors not found: {fromConnectorName} -> {toConnectorName}");
            return false;
        }

        return ConnectionManager.Instance.CreateConnectionWithConnectors(fromConnector, toConnector);
    }

    private Connector FindConnectorByAttributeName(List<Connector> connectors, string attributeName)
    {
        foreach (var connector in connectors)
        {
            var attribute = connector.Field?.GetAttribute();
            if (attribute != null && attribute.attributeName == attributeName)
            {
                return connector;
            }
        }
        return null;
    }

    private void LoadAllValues(Dictionary<int, (NodeLogic nodeLogic, object valueToLoad)> loadedNodes)
    {
        foreach (var kvp in loadedNodes)
        {
            var nodeLogic = kvp.Value.nodeLogic;
            var valueToLoad = kvp.Value.valueToLoad;

            if (nodeLogic != null && nodeLogic.Node != null && valueToLoad != null)
            {
                if (nodeLogic.Node is VariableNode variableNode)
                {
                    variableNode.SetValue(valueToLoad);
                    variableNode.SyncUIWithCachedValue();
                }
            }
        }
    }

    private object ParseNodeValue(NodeInstanceData nodeSaveData)
    {
        if (string.IsNullOrEmpty(nodeSaveData.fieldValuesJson))
            return null;

        try
        {
            var fieldValuesList = JsonUtility.FromJson<FieldValueDataList>(nodeSaveData.fieldValuesJson);
            var variableValueData = fieldValuesList?.values?.FirstOrDefault(v => v.attributeName == "variableValue");

            if (variableValueData != null && !string.IsNullOrEmpty(variableValueData.value))
            {
                // Find appropriate persister based on variableType
                var persister = _valuePersisters.FirstOrDefault(p => p.CanPersist(nodeSaveData.variableType));

                if (persister != null)
                {
                    return persister.RestoreValue(variableValueData.value, nodeSaveData.variableType);
                }
                else
                {
                    // Fallback to default deserialization
                    return DeserializeVariableValue(nodeSaveData.variableType, variableValueData.value);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse node value: {e.Message}");
        }

        return null;
    }

    private object DeserializeVariableValue(VariableType type, string value)
    {
        if (string.IsNullOrEmpty(value))
            return GetDefaultValueForType(type);

        try
        {
            switch (type)
            {
                case VariableType.Bool:
                    return bool.Parse(value);
                case VariableType.Int:
                    return int.Parse(value);
                case VariableType.Single:
                    // Use invariant culture for floats
                    return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                case VariableType.String:
                    return value;
                case VariableType.Vector3:
                    return JsonUtility.FromJson<Vector3>(value);
                case VariableType.Type:
                    return TypeSerializer.DeserializeType(value);
                default:
                    return GetDefaultValueForType(type);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to deserialize {type} value '{value}': {e.Message}");
            return GetDefaultValueForType(type);
        }
    }

    private void ClearCurrentGraph()
    {
        NodeSpawnerService.Instance.ClearAllNodes();
        if (ConnectionManager.Instance != null)
        {
            ConnectionManager.Instance.ClearAllConnections();
        }
    }

    public void QuickSave() => SaveGraph("quicksave");
    public void QuickLoad() => LoadGraph("quicksave");

    public List<string> GetSaveFiles()
    {
        var saveFiles = new List<string>();
        string saveDirectory = Application.dataPath;
        if (!Directory.Exists(saveDirectory)) return saveFiles;

        var files = Directory.GetFiles(saveDirectory, "*.json");
        foreach (var file in files)
        {
            saveFiles.Add(Path.GetFileNameWithoutExtension(file));
        }
        return saveFiles;
    }

    public void DeleteSaveFile(string saveName)
    {
        try
        {
            string filePath = GetSavePath(saveName);
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Delete failed: {e.Message}");
        }
    }

    public void DeleteAllSaves()
    {
        try
        {
            var saveFiles = GetSaveFiles();
            foreach (var saveFile in saveFiles) DeleteSaveFile(saveFile);
        }
        catch (Exception e)
        {
            Debug.LogError($"Delete all failed: {e.Message}");
        }
    }

    private void SaveNodeData(NodeBase node, NodeInstanceData nodeSaveData)
    {
        var fieldValues = new List<FieldValueData>();

        if (node is VariableNode variableNode)
        {
            // Find appropriate persister
            var persister = _valuePersisters.FirstOrDefault(p => p.CanPersist(variableNode.VariableType));

            string serializedValue;
            if (persister != null)
            {
                serializedValue = persister.PersistValue(variableNode.GetValue());
            }
            else
            {
                // Fallback to default serialization
                serializedValue = SerializeVariableValue(variableNode);
            }

            fieldValues.Add(new FieldValueData
            {
                attributeName = "variableValue",
                value = serializedValue
            });
        }

        nodeSaveData.fieldValuesJson = JsonUtility.ToJson(new FieldValueDataList { values = fieldValues });
    }

    private string SerializeVariableValue(VariableNode node)
    {
        object val = node.GetValue();
        if (val == null) return string.Empty;

        switch (node.VariableType)
        {
            case VariableType.Bool:
            case VariableType.Int:
            case VariableType.String:
                return val.ToString();
            case VariableType.Single:
                // Use invariant culture for floats
                return ((float)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
            case VariableType.Vector3:
                return JsonUtility.ToJson((Vector3)val);
            case VariableType.Type:
                var typeValue = val as Type;
                return TypeSerializer.SerializeType(typeValue);
            default:
                return val.ToString();
        }
    }

    private object GetDefaultValueForType(VariableType type)
    {
        switch (type)
        {
            case VariableType.Bool: return false;
            case VariableType.Int: return 0;
            case VariableType.Single: return 0f;
            case VariableType.String: return "";
            case VariableType.Vector3: return Vector3.zero;
            case VariableType.Type: return typeof(object);
            default: return null;
        }
    }

    private string GetSavePath(string saveName)
    {
        return Path.Combine(Application.dataPath, $"{saveName}.json");
    }
}