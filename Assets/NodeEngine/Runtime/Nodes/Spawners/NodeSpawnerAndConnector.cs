using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NodeSpawnerAndConnector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NodesDatabase _nodesDatabase;
    [SerializeField] private bool _enabled = false;

    private void Update()
    {
        if (!_enabled)
            return;

        if (Keyboard.current.quoteKey.wasReleasedThisFrame)
        {
            SpawnAndConnectNodes();
        }
    }

    private void SpawnAndConnectNodes()
    {
        var nodes = _nodesDatabase.GetNodes();

        var updateNode = nodes.FirstOrDefault(n => n.GetType().Name == "UpdateNode");
        var intVariableNode = nodes.FirstOrDefault(n => n is IntVariableNode);
        var forLoopNode = nodes.FirstOrDefault(n => n.GetType().Name == "ForLoopNode");
        var toStringNode = nodes.FirstOrDefault(n => n.GetType().Name == "ToStringNode");
        var debugNode = nodes.FirstOrDefault(n => n.GetType().Name == "DebugNode");

        if (updateNode == null || intVariableNode == null || forLoopNode == null || toStringNode == null || debugNode == null)
        {
            Debug.LogWarning("One or more required nodes not found in database");
            return;
        }

        Vector2 startPos = new Vector2(0, 0);
        float spacing = 200f;

        var updateNodeLogic = NodeSpawnerService.Instance.SpawnNode(_nodesDatabase.GetClone(updateNode), startPos);
        var intNodeLogic = NodeSpawnerService.Instance.SpawnNode(_nodesDatabase.GetClone(intVariableNode), startPos + new Vector2(spacing, 50));
        var forLoopNodeLogic = NodeSpawnerService.Instance.SpawnNode(_nodesDatabase.GetClone(forLoopNode), startPos + new Vector2(spacing * 2, 0));
        var toStringNodeLogic = NodeSpawnerService.Instance.SpawnNode(_nodesDatabase.GetClone(toStringNode), startPos + new Vector2(spacing * 3, 50));
        var debugNodeLogic = NodeSpawnerService.Instance.SpawnNode(_nodesDatabase.GetClone(debugNode), startPos + new Vector2(spacing * 4, 0));

        if (updateNodeLogic == null || intNodeLogic == null || forLoopNodeLogic == null || toStringNodeLogic == null || debugNodeLogic == null)
        {
            Debug.LogWarning("Failed to spawn one or more nodes");
            return;
        }

        ConnectNodes(updateNodeLogic, intNodeLogic, forLoopNodeLogic, toStringNodeLogic, debugNodeLogic);
    }

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
        {
            Debug.LogWarning("Some node connections failed");
        }
    }

    private bool TryConnectWithFallback(NodeLogic fromNode, NodeLogic toNode, string fromConnectorName, string toConnectorName, Type fromType, Type toType)
    {
        // Find connectors on the NodeLogic view directly
        var fromConn = fromNode.OutputConnectors.FirstOrDefault(c => c.PortName == fromConnectorName);
        var toConn = toNode.InputConnectors.FirstOrDefault(c => c.PortName == toConnectorName);

        if (fromConn != null && toConn != null)
        {
            return ConnectionManager.Instance.CreateConnectionWithConnectors(fromConn, toConn);
        }

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
                {
                    return ConnectionManager.Instance.CreateConnectionWithConnectors(outputConnector, inputConnector);
                }
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