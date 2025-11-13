using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Radishmouse;  // Ensure this is imported for UILineRenderer

public class NodeSpawnerAndConnector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NodesDatabase _nodesDatabase;
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private VariableDatabase _variableDatabase;
    [SerializeField] private UILineRenderer _lineRendererPrefab;  // NEW: Assign the UILineRenderer prefab from ConnectorDragLogic

    [Header("Settings")]
    [SerializeField] private float _spacing = 200f;  // Horizontal spacing between nodes

    private List<NodeLogic> _spawnedNodes = new List<NodeLogic>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnAndConnectNodes();
        }
    }

    private void SpawnAndConnectNodes()
    {
        if (_nodesDatabase == null || _nodeLogicPrefab == null || _variableDatabase == null || _lineRendererPrefab == null)
        {
            Debug.LogError("Missing dependencies in NodeSpawnerAndConnector (check NodesDatabase, NodeLogic prefab, VariableDatabase, and UILineRenderer prefab)");
            return;
        }

        // Clear previous spawns
        foreach (var node in _spawnedNodes)
        {
            if (node != null) node.DeleteNode();
        }
        _spawnedNodes.Clear();

        // Get node types
        var nodes = _nodesDatabase.GetNodes();
        var updateNodeType = nodes.FirstOrDefault(n => n.GetType().Name == "UpdateNode");
        var forLoopNodeType = nodes.FirstOrDefault(n => n.GetType().Name == "ForLoopNode");
        var toStringNodeType = nodes.FirstOrDefault(n => n.GetType().Name == "ToStringNode");
        var debugNodeType = nodes.FirstOrDefault(n => n.GetType().Name == "DebugNode");

        if (updateNodeType == null || forLoopNodeType == null || toStringNodeType == null || debugNodeType == null)
        {
            Debug.LogError("Required node types not found in NodesDatabase");
            return;
        }

        // Spawn nodes
        Vector2 startPos = new Vector2(0, 0);
        var updateNode = SpawnNode(updateNodeType, startPos);
        var intNode = SpawnVariableNode(VariableType.Int, startPos + new Vector2(_spacing, 0));
        var forLoopNode = SpawnNode(forLoopNodeType, startPos + new Vector2(_spacing * 2, 0));
        var toStringNode = SpawnNode(toStringNodeType, startPos + new Vector2(_spacing * 3, 0));
        var debugNode = SpawnNode(debugNodeType, startPos + new Vector2(_spacing * 4, 0));

        _spawnedNodes.AddRange(new[] { updateNode, intNode, forLoopNode, toStringNode, debugNode });

        // Connect nodes
        ConnectNodes(updateNode, forLoopNode, typeof(void), typeof(void));  // Update -> ForLoop (Execute)
        ConnectNodes(intNode, forLoopNode, typeof(int), typeof(int));      // Int -> ForLoop (Count)
        ConnectNodes(forLoopNode, toStringNode, typeof(int), typeof(object));  // ForLoop (Index) -> ToString (Input)
        ConnectNodes(toStringNode, debugNode, typeof(string), typeof(string)); // ToString -> Debug (LogString)
        ConnectNodes(forLoopNode, debugNode, typeof(void), typeof(void));     // ForLoop (Body) -> Debug (Event)

        Debug.Log("Nodes spawned and connected successfully!");
    }

    private NodeLogic SpawnNode(NodeBase nodeType, Vector2 position)
    {
        var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;
        nodeLogic.VariableDatabase = _variableDatabase;
        nodeLogic.SetNodeBase(nodeType);
        return nodeLogic;
    }

    private NodeLogic SpawnVariableNode(VariableType type, Vector2 position)
    {
        var variableNode = new VariableNode();
        variableNode.VariableType = type;  // Set type
        var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;
        nodeLogic.VariableDatabase = _variableDatabase;
        nodeLogic.SetNodeBase(variableNode);

        // Set default value (e.g., 5 for Int)
        if (variableNode.UIElement is InputFieldVariableUI inputField)
        {
            inputField.SetValue(5);  // Adjust as needed
        }

        return nodeLogic;
    }

    private void ConnectNodes(NodeLogic fromNode, NodeLogic toNode, System.Type outputType, System.Type inputType)
    {
        var fromConnector = fromNode.Node.outputConnectors.FirstOrDefault(c => c.ValueType == outputType);
        var toConnector = toNode.Node.inputConnectors.FirstOrDefault(c => c.ValueType == inputType);

        if (fromConnector == null || toConnector == null)
        {
            Debug.LogWarning($"Failed to find connectors for types {outputType} -> {inputType}");
            return;
        }

        // Instantiate and add the line renderer
        var lineRenderer = Instantiate(_lineRendererPrefab, LineRenderersController.instance.transform);  // Instantiate under the controller
        LineRenderersController.Add(fromConnector, toConnector, lineRenderer);

        // Update connections
        fromConnector.AddConnection(toConnector);
        fromConnector.UpdateFilled();
        toConnector.AddConnection(fromConnector);
        toConnector.UpdateFilled();
    }
}