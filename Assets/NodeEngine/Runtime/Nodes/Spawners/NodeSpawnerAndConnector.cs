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
        else
        {
            Debug.Log("All node connections successful!");
        }
    }

    private bool TryConnectWithFallback(NodeLogic fromNode, NodeLogic toNode, string fromConnectorName, string toConnectorName, Type fromType, Type toType)
    {
        Debug.Log($"Trying to connect: {fromNode.Node.NodeName}.{fromConnectorName} → {toNode.Node.NodeName}.{toConnectorName}");

        if (ConnectionManager.Instance.CreateConnection(fromNode, toNode, fromConnectorName, toConnectorName))
        {
            Debug.Log($"Connected via names: {fromConnectorName} → {toConnectorName}");
            return true;
        }

        if (ConnectionManager.Instance.CreateConnection(fromNode, toNode, fromType, toType))
        {
            Debug.Log($"Connected via types: {fromType.Name} → {toType.Name}");
            return true;
        }

        Debug.Log($"Falling back to compatible connection for: {fromType.Name} → {toType.Name}");
        return TryConnectAnyCompatible(fromNode, toNode, fromType, toType);
    }

    private bool TryConnectAnyCompatible(NodeLogic fromNode, NodeLogic toNode, Type preferredFromType, Type preferredToType)
    {
        if (fromNode?.Node?.outputConnectors == null || toNode?.Node?.inputConnectors == null)
            return false;

        Debug.Log($"Searching for compatible connectors between {fromNode.Node.NodeName} and {toNode.Node.NodeName}");

        foreach (var outputConnector in fromNode.Node.outputConnectors)
        {
            var outputAttr = outputConnector.Field?.GetAttribute();
            Debug.Log($"  Output: {outputAttr?.attributeName ?? "Unknown"} ({outputConnector.ValueType.Name})");

            foreach (var inputConnector in toNode.Node.inputConnectors)
            {
                var inputAttr = inputConnector.Field?.GetAttribute();
                Debug.Log($"    Input: {inputAttr?.attributeName ?? "Unknown"} ({inputConnector.ValueType.Name})");

                if (IsCompatibleType(outputConnector.ValueType, inputConnector.ValueType))
                {
                    Debug.Log($"    Found compatible: {outputAttr?.attributeName} → {inputAttr?.attributeName}");
                    return ConnectionManager.Instance.CreateConnectionWithConnectors(outputConnector, inputConnector);
                }
            }
        }

        Debug.Log("No compatible connectors found");
        return false;
    }

    private bool IsCompatibleType(Type dragType, Type targetType)
    {
        bool compatible = dragType == targetType || targetType == typeof(object);
        Debug.Log($"Type compatibility: {dragType.Name} → {targetType.Name} = {compatible}");
        return compatible;
    }
}