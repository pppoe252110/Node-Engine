using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class NodeSpawnerAndConnector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private bool _enabled = false;

    private NodesDatabase _nodesDatabase;
    private NodeSpawnerService _nodeSpawnerService;
    private ConnectionManager _connectionManager;
    private INodeFactory _nodeFactory;

    [Inject]
    public void Construct(
        NodeSpawnerService spawnerService,
        NodesDatabase nodesDatabase,
        ConnectionManager connectionManager,
        INodeFactory nodeFactory)
    {
        _nodesDatabase = nodesDatabase;
        _nodeSpawnerService = spawnerService;
        _connectionManager = connectionManager;
        _nodeFactory = nodeFactory;
    }

    private void Update()
    {
        if (!_enabled) return;

        if (Keyboard.current.quoteKey.wasReleasedThisFrame)
        {
            SpawnAndConnectNodes();
        }
    }

    private void SpawnAndConnectNodes()
    {
        var allNodeData = _nodesDatabase.GetAllNodeData().ToList();

        // Find required node data by their type names (or custom criteria)
        var updateData = FindNodeDataByTypeName(allNodeData, "UpdateNode");
        var intVarData = FindNodeDataByTypeName(allNodeData, "IntVariableNode");
        var forLoopData = FindNodeDataByTypeName(allNodeData, "ForLoopNode");
        var toStringData = FindNodeDataByTypeName(allNodeData, "ToStringNode");
        var debugData = FindNodeDataByTypeName(allNodeData, "DebugNode");

        if (updateData == null || intVarData == null || forLoopData == null ||
            toStringData == null || debugData == null)
        {
            Debug.LogWarning("One or more required node types not found in database");
            return;
        }

        Vector2 startPos = new Vector2(0, 0);
        float spacing = 200f;

        var updateLogic = SpawnNodeFromData(updateData, startPos);
        var intLogic = SpawnNodeFromData(intVarData, startPos + new Vector2(spacing, 50));
        var forLoopLogic = SpawnNodeFromData(forLoopData, startPos + new Vector2(spacing * 2, 0));
        var toStringLogic = SpawnNodeFromData(toStringData, startPos + new Vector2(spacing * 3, 50));
        var debugLogic = SpawnNodeFromData(debugData, startPos + new Vector2(spacing * 4, 0));

        if (updateLogic == null || intLogic == null || forLoopLogic == null ||
            toStringLogic == null || debugLogic == null)
        {
            Debug.LogWarning("Failed to spawn one or more nodes");
            return;
        }

        ConnectNodes(updateLogic, intLogic, forLoopLogic, toStringLogic, debugLogic);
    }

    private SerializableNode FindNodeDataByTypeName(System.Collections.Generic.IEnumerable<SerializableNode> nodeData, string typeName)
    {
        return nodeData.FirstOrDefault(data =>
        {
            Type t = Type.GetType(data.nodeType);
            return t != null && t.Name == typeName;
        });
    }

    private NodeLogic SpawnNodeFromData(SerializableNode nodeData, Vector2 position)
    {
        Type type = Type.GetType(nodeData.nodeType);
        if (type == null)
        {
            Debug.LogError($"Cannot resolve type: {nodeData.nodeType}");
            return null;
        }

        // Create a fresh instance via factory
        BaseNode instance = _nodeFactory.CreateNode(type);

        // Apply metadata (name, icon) from database
        _nodesDatabase.ApplyMetadata(instance);

        // Spawn visual node
        return _nodeSpawnerService.SpawnNode(instance, position);
    }

    // The rest of the connection logic remains unchanged
    private void ConnectNodes(NodeLogic updateNode, NodeLogic intNode, NodeLogic forLoopNode,
                             NodeLogic toStringNode, NodeLogic debugNode)
    {
        bool allSuccess = true;

        // Execution flow: Update → ForLoop
        if (!TryConnectWithFallback(updateNode, forLoopNode, "Update", "Input", typeof(void), typeof(void)))
            allSuccess = false;

        // Data flow: IntVariable → ForLoop Count
        if (!TryConnectWithFallback(intNode, forLoopNode, "Value", "Count", typeof(int), typeof(int)))
            allSuccess = false;

        // Data flow: ForLoop Index → ToString Input
        if (!TryConnectWithFallback(forLoopNode, toStringNode, "Index", "Inputvalue", typeof(int), typeof(object)))
            allSuccess = false;

        // Execution flow: ForLoop Body → ToString Input (execution)
        if (!TryConnectWithFallback(forLoopNode, toStringNode, "Body", "Input", typeof(void), typeof(void)))
            allSuccess = false;

        // Data flow: ToString Output → Debug LogString
        if (!TryConnectWithFallback(toStringNode, debugNode, "OutputValue", "LogString", typeof(string), typeof(string)))
            allSuccess = false;

        // Execution flow: ToString Output (execution) → Debug Input (execution)
        if (!TryConnectWithFallback(toStringNode, debugNode, "Output", "Input", typeof(void), typeof(void)))
            allSuccess = false;

        if (!allSuccess)
            Debug.LogWarning("Some node connections failed");
    }

    private bool TryConnectWithFallback(NodeLogic fromNode, NodeLogic toNode, string fromConnectorName, string toConnectorName, Type fromType, Type toType)
    {
        var fromConn = fromNode.OutputConnectors.FirstOrDefault(c => c.PortName == fromConnectorName);
        var toConn = toNode.InputConnectors.FirstOrDefault(c => c.PortName == toConnectorName);

        if (fromConn != null && toConn != null)
            return _connectionManager.CreateConnectionWithConnectors(fromConn, toConn);

        Debug.Log($"Falling back to compatible connection for: {fromType.Name} → {toType.Name}");
        return TryConnectAnyCompatible(fromNode, toNode, fromType, toType);
    }

    private bool TryConnectAnyCompatible(NodeLogic fromNode, NodeLogic toNode, Type preferredFromType, Type preferredToType)
    {
        if (fromNode?.OutputConnectors == null || toNode?.InputConnectors == null) return false;

        foreach (var outputConnector in fromNode.OutputConnectors)
        {
            foreach (var inputConnector in toNode.InputConnectors)
            {
                if (IsCompatibleType(outputConnector.ValueType, inputConnector.ValueType))
                    return _connectionManager.CreateConnectionWithConnectors(outputConnector, inputConnector);
            }
        }

        Debug.Log("No compatible connectors found");
        return false;
    }

    private bool IsCompatibleType(Type dragType, Type targetType)
    {
        return TypeChangeLogic.IsCompatibleType(dragType, targetType);
    }
}