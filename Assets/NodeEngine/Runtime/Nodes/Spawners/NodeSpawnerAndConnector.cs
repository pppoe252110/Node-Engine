using Radishmouse;  
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NodeSpawnerAndConnector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NodesDatabase _nodesDatabase;
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private UILineRenderer _lineRendererPrefab;  

    [Header("Settings")]
    [SerializeField] private float _spacing = 200f;  

    private List<NodeLogic> _spawnedNodes = new List<NodeLogic>();

    private void Update()
    {
        if (Keyboard.current.quoteKey.wasReleasedThisFrame)
        {
            SpawnAndConnectNodes();
        }
    }

    private void SpawnAndConnectNodes()
    {
        if (_nodesDatabase == null || _nodeLogicPrefab == null || _lineRendererPrefab == null)
        {
            Debug.LogError("Missing dependencies in NodeSpawnerAndConnector (check NodesDatabase, NodeLogic prefab, VariableDatabase, and UILineRenderer prefab)");
            return;
        }

        
        foreach (var node in _spawnedNodes)
        {
            if (node != null) node.DeleteNode();
        }
        _spawnedNodes.Clear();

        
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

        
        Vector2 startPos = new Vector2(0, 0);
        var updateNode = SpawnNode(updateNodeType, startPos);
        var intNode = SpawnVariableNode(VariableType.Int, startPos + new Vector2(_spacing, 0));
        var forLoopNode = SpawnNode(forLoopNodeType, startPos + new Vector2(_spacing * 2, 0));
        var toStringNode = SpawnNode(toStringNodeType, startPos + new Vector2(_spacing * 3, 0));
        var debugNode = SpawnNode(debugNodeType, startPos + new Vector2(_spacing * 4, 0));

        _spawnedNodes.AddRange(new[] { updateNode, intNode, forLoopNode, toStringNode, debugNode });

        
        ConnectNodes(updateNode, forLoopNode, typeof(void), typeof(void));  
        ConnectNodes(intNode, forLoopNode, typeof(int), typeof(int));      
        ConnectNodes(forLoopNode, toStringNode, typeof(int), typeof(object));  
        ConnectNodes(toStringNode, debugNode, typeof(string), typeof(string)); 
        ConnectNodes(forLoopNode, debugNode, typeof(void), typeof(void));     

        Debug.Log("Nodes spawned and connected successfully!");
    }

    private NodeLogic SpawnNode(NodeBase nodeType, Vector2 position)
    {
        var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;
        nodeLogic.SetNodeBase(nodeType);
        return nodeLogic;
    }

    private NodeLogic SpawnVariableNode(VariableType type, Vector2 position)
    {
        var variableNode = new IntVariableNode();
        var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);

        nodeLogic.transform.localPosition = position;
        nodeLogic.SetNodeBase(variableNode);

        
        if (variableNode.UIElement is InputFieldVariableUI inputField)
        {
            inputField.SetValue(5);  
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

        
        var lineRenderer = Instantiate(_lineRendererPrefab, LineRenderersController.Instance.transform);  
        LineRenderersController.Add(fromConnector, toConnector, lineRenderer);

        
        fromConnector.AddConnection(toConnector);
        fromConnector.UpdateFilled();
        toConnector.AddConnection(fromConnector);
        toConnector.UpdateFilled();
    }
}