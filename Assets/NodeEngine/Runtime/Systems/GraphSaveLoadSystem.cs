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

    [Header("References")]
    [SerializeField] private NodesDatabase _nodesDatabase;

    public event Action<string> OnGraphSaved;
    public event Action<string> OnGraphLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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

            OnGraphSaved?.Invoke(saveName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save graph: {e.Message}");
        }
    }

    public void LoadGraph(string saveName)
    {
        if (string.IsNullOrEmpty(saveName)) return;

        try
        {
            string filePath = GetSavePath(saveName);
            if (!File.Exists(filePath)) return;

            string json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<GraphSaveData>(json);

            LoadFromSaveData(saveData);

            OnGraphLoaded?.Invoke(saveName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load graph: {e.Message}");
        }
    }

    private GraphSaveData CreateSaveData()
    {
        var saveData = new GraphSaveData
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
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
            };

            SaveNodeData(nodeLogic.Node, nodeSaveData);
            saveData.nodes.Add(nodeSaveData);
        }

        foreach (var connection in ConnectionManager.Instance.GetAllConnections())
        {
            saveData.connections.Add(new ConnectionSaveData
            {
                fromNodeId = connection.fromNodeId,
                toNodeId = connection.toNodeId,
                fromConnectorName = connection.fromConnectorName,
                toConnectorName = connection.toConnectorName
            });
        }
        return saveData;
    }

    private string FindDatabaseIdForNode(NodeBase node)
    {
        var nodes = _nodesDatabase.GetNodes();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].GetType() == node.GetType() &&
                nodes[i].NodeName == node.NodeName)
            {
                return $"{node.GetType().Name}_{node.NodeName}";
            }
        }
        return "unknown";
    }

    private void LoadFromSaveData(GraphSaveData saveData)
    {
        ClearCurrentGraph();

        var loadedNodeIds = new HashSet<int>();
        var databaseNodes = _nodesDatabase.GetNodes().ToDictionary(
            n => $"{n.GetType().Name}_{n.NodeName}",
            n => n
        );

        foreach (var nodeSaveData in saveData.nodes)
        {
            if (databaseNodes.TryGetValue(nodeSaveData.databaseId, out var databaseNode))
            {
                var nodeLogic = NodeSpawnerService.Instance.SpawnNode(databaseNode, nodeSaveData.position, nodeSaveData.instanceId);

                if (nodeLogic != null)
                {
                    LoadNodeData(nodeLogic.Node, nodeSaveData);
                    loadedNodeIds.Add(nodeSaveData.instanceId);
                }
            }
        }

        ConnectNodes(saveData, loadedNodeIds);
    }

    private void ConnectNodes(GraphSaveData saveData, HashSet<int> loadedNodeIds)
    {
        foreach (var connection in saveData.connections)
        {
            if (loadedNodeIds.Contains(connection.fromNodeId) && loadedNodeIds.Contains(connection.toNodeId))
            {
                ConnectNodesByAttributeNames(
                    connection.fromNodeId,
                    connection.toNodeId,
                    connection.fromConnectorName,
                    connection.toConnectorName
                );
            }
        }
    }

    private bool ConnectNodesByAttributeNames(int fromNodeId, int toNodeId, string fromConnectorName, string toConnectorName)
    {
        var fromNode = NodeSpawnerService.Instance.GetNodeById(fromNodeId);
        var toNode = NodeSpawnerService.Instance.GetNodeById(toNodeId);

        if (fromNode == null || toNode == null) return false;

        var fromConnector = fromNode.Node.outputConnectors.FirstOrDefault(c =>
        {
            var attr = c.Field?.GetAttribute();
            return attr?.attributeName == fromConnectorName;
        });

        var toConnector = toNode.Node.inputConnectors.FirstOrDefault(c =>
        {
            var attr = c.Field?.GetAttribute();
            return attr?.attributeName == toConnectorName;
        });

        if (fromConnector == null || toConnector == null) return false;

        return ConnectionManager.Instance.CreateConnectionWithConnectors(fromConnector, toConnector);
    }

    private void ClearCurrentGraph()
    {
        NodeSpawnerService.Instance.ClearAllNodes();
        ConnectionManager.Instance.ClearAllConnections();
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
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }

    public void DeleteAllSaves()
    {
        try
        {
            var saveFiles = GetSaveFiles();
            foreach (var saveFile in saveFiles)
            {
                DeleteSaveFile(saveFile);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete all saves: {e.Message}");
        }
    }

    private void SaveNodeData(NodeBase node, NodeInstanceData nodeSaveData)
    {
        var fieldValues = new List<FieldValueData>();

        if (node is VariableNode variableNode)
        {
            if (variableNode.UIElement != null)
            {
                fieldValues.Add(new FieldValueData
                {
                    attributeName = "variableValue",
                    value = variableNode.UIElement.GetValue()?.ToString()
                });
            }
        }

        nodeSaveData.fieldValuesJson = JsonUtility.ToJson(new FieldValueDataList { values = fieldValues });
    }

    private void LoadNodeData(NodeBase node, NodeInstanceData nodeSaveData)
    {
        // Try to get the field values from the nodeSaveData
        if (!string.IsNullOrEmpty(nodeSaveData.fieldValuesJson))
        {
            try
            {
                // Deserialize the list of FieldValueData
                var fieldValuesList = JsonUtility.FromJson<FieldValueDataList>(nodeSaveData.fieldValuesJson);
                if (fieldValuesList?.values != null)
                {
                    // Handle VariableNode special case
                    if (node is VariableNode variableNode)
                    {
                        var variableValueData = fieldValuesList.values.FirstOrDefault();
                        if (variableValueData != null && variableNode.UIElement is InputFieldVariableUI inputField)
                        {
                            Debug.LogError(variableValueData.value);
                            inputField.UpdateValue(variableValueData.value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load node data: {e.Message}");
            }
        }
    }

    private string GetSavePath(string saveName)
    {
        return Path.Combine(Application.dataPath, $"{saveName}.json");
    }
}