using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NodeSpawnerAndConnector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NodesDatabase _nodesDatabase;

    private void Update()
    {
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
            return;
        }

        Vector2 startPos = new Vector2(0, 0);
        float spacing = 200f;

        var updateNodeLogic = NodeSpawnerService.Instance.SpawnNode(updateNode, startPos);
        var intNodeLogic = NodeSpawnerService.Instance.SpawnNode(intVariableNode, startPos + new Vector2(spacing, 50));
        var forLoopNodeLogic = NodeSpawnerService.Instance.SpawnNode(forLoopNode, startPos + new Vector2(spacing * 2, 0));
        var toStringNodeLogic = NodeSpawnerService.Instance.SpawnNode(toStringNode, startPos + new Vector2(spacing * 3, 50));
        var debugNodeLogic = NodeSpawnerService.Instance.SpawnNode(debugNode, startPos + new Vector2(spacing * 4, 0));

        if (updateNodeLogic == null || intNodeLogic == null || forLoopNodeLogic == null || toStringNodeLogic == null || debugNodeLogic == null)
        {
            return;
        }

        ConnectNodes(updateNodeLogic, intNodeLogic, forLoopNodeLogic, toStringNodeLogic, debugNodeLogic);
    }

    private void ConnectNodes(NodeLogic updateNode, NodeLogic intNode, NodeLogic forLoopNode, NodeLogic toStringNode, NodeLogic debugNode)
    {
        TryConnectNodes(updateNode, forLoopNode, intNode, toStringNode, debugNode);
    }

    private bool TryConnectNodes(NodeLogic updateNode, NodeLogic forLoopNode, NodeLogic intNode, NodeLogic toStringNode, NodeLogic debugNode)
    {
        bool allSuccess = true;

        if (!TryConnectWithFallback(updateNode, forLoopNode, "Update", "Execute", typeof(void), typeof(void)))
            allSuccess = false;

        if (!TryConnectWithFallback(intNode, forLoopNode, "Value", "Count", typeof(int), typeof(int)))
            allSuccess = false;

        if (!TryConnectWithFallback(forLoopNode, toStringNode, "Index", "Input", typeof(int), typeof(object)))
            allSuccess = false;

        if (!TryConnectWithFallback(toStringNode, debugNode, "Output", "LogString", typeof(string), typeof(string)))
            allSuccess = false;

        if (!TryConnectWithFallback(forLoopNode, debugNode, "Body", "Execute", typeof(void), typeof(void)))
            allSuccess = false;

        return allSuccess;
    }

    private bool TryConnectWithFallback(NodeLogic fromNode, NodeLogic toNode, string fromConnectorName, string toConnectorName, Type fromType, Type toType)
    {
        if (ConnectionManager.Instance.CreateConnection(fromNode, toNode, fromConnectorName, toConnectorName))
            return true;

        if (ConnectionManager.Instance.CreateConnection(fromNode, toNode, fromType, toType))
            return true;

        return TryConnectAnyCompatible(fromNode, toNode, fromType, toType);
    }

    private bool TryConnectAnyCompatible(NodeLogic fromNode, NodeLogic toNode, Type preferredFromType, Type preferredToType)
    {
        if (fromNode?.Node?.outputConnectors == null || toNode?.Node?.inputConnectors == null)
            return false;

        foreach (var outputConnector in fromNode.Node.outputConnectors)
        {
            foreach (var inputConnector in toNode.Node.inputConnectors)
            {
                if (IsCompatibleType(outputConnector.ValueType, inputConnector.ValueType))
                {
                    return ConnectionManager.Instance.CreateConnectionWithConnectors(outputConnector, inputConnector);
                }
            }
        }

        return false;
    }

    private bool IsCompatibleType(Type outputType, Type inputType)
    {
        if (outputType == inputType) return true;
        if (inputType == typeof(object)) return true;
        if (outputType == typeof(void) && inputType == typeof(void)) return true;

        if ((outputType == typeof(int) || outputType == typeof(float)) &&
            (inputType == typeof(int) || inputType == typeof(float)))
            return true;

        return false;
    }
}