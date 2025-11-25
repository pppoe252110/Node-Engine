using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeSpawnerService : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NodesDatabase _nodesDatabase;
    [SerializeField] private VariableDatabase _variableDatabase;
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private ConnectorColorDatabase _colorDatabase;

    [Header("Connector Prefabs")]
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;

    private Dictionary<int, NodeLogic> _spawnedNodes = new Dictionary<int, NodeLogic>();
    private int _nextNodeId = 1;

    public static NodeSpawnerService Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    public NodeLogic SpawnNode(NodeBase nodeInstance, Vector2 position, int nodeId = -1)
    {
        if (nodeInstance == null)
        {
            return null;
        }

        if (nodeId == -1)
        {
            nodeId = _nextNodeId++;
        }
        else
        {
            _nextNodeId = Mathf.Max(_nextNodeId, nodeId + 1);
        }

        var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;
        nodeLogic.VariableDatabase = _variableDatabase;

        nodeLogic.SetNodeBase(nodeInstance);

        _spawnedNodes[nodeId] = nodeLogic;

        return nodeLogic;
    }

    public void GenerateConnectors(NodeLogic nodeLogic, NodeBase node)
    {
        if (node is VariableNode varNode)
        {
            GenerateVariableNodeConnectors(nodeLogic, varNode);
        }
        else
        {
            GenerateRegularNodeConnectors(nodeLogic, node);
        }
    }

    private void GenerateVariableNodeConnectors(NodeLogic nodeLogic, VariableNode varNode)
    {
        ClearConnectors(nodeLogic);

        foreach (var field in varNode.outputFields)
        {
            var connector = Instantiate(_rightConnectorPrefab, nodeLogic.RightConnectorsParent);
            SetupConnector(connector, field, varNode);
            nodeLogic.Node.outputConnectors.Add(connector);
        }
    }

    private void GenerateRegularNodeConnectors(NodeLogic nodeLogic, NodeBase node)
    {
        ClearConnectors(nodeLogic);

        foreach (var field in node.inputFields)
        {
            var connector = Instantiate(_leftConnectorPrefab, nodeLogic.LeftConnectorsParent);
            SetupConnector(connector, field, node);
            nodeLogic.Node.inputConnectors.Add(connector);
        }

        foreach (var field in node.outputFields)
        {
            var connector = Instantiate(_rightConnectorPrefab, nodeLogic.RightConnectorsParent);
            SetupConnector(connector, field, node);
            nodeLogic.Node.outputConnectors.Add(connector);
        }
    }

    private void SetupConnector(Connector connector, NodeFieldBase field, NodeBase node)
    {
        connector.SetField(field);
        connector.SetNode(node);
        connector.SetColorDatabase(_colorDatabase);

        var attribute = field.GetAttribute();
        connector.SetData(attribute);
    }

    private void ClearConnectors(NodeLogic nodeLogic)
    {
        foreach (var connector in nodeLogic.Node.inputConnectors)
        {
            if (connector != null) Destroy(connector.gameObject);
        }
        foreach (var connector in nodeLogic.Node.outputConnectors)
        {
            if (connector != null) Destroy(connector.gameObject);
        }

        nodeLogic.Node.inputConnectors.Clear();
        nodeLogic.Node.outputConnectors.Clear();
    }

    public NodeLogic SpawnNodeFromDatabase(string nodeTypeName, Vector2 position, int nodeId = -1)
    {
        var nodes = _nodesDatabase.GetNodes();
        var nodeInstance = nodes.FirstOrDefault(n => n.GetType().Name == nodeTypeName);

        if (nodeInstance != null)
        {
            return SpawnNode(nodeInstance, position, nodeId);
        }

        return null;
    }

    public void DeleteNode(NodeLogic nodeLogic)
    {
        if (nodeLogic == null) return;

        var nodeId = _spawnedNodes.FirstOrDefault(x => x.Value == nodeLogic).Key;
        if (nodeId != 0)
        {
            _spawnedNodes.Remove(nodeId);
        }

        var allConnectors = nodeLogic.Node.inputConnectors.Concat(nodeLogic.Node.outputConnectors);
        foreach (var connector in allConnectors)
        {
            foreach (var connectedConnector in connector.Connections.ToArray())
            {
                LineRenderersController.Remove(connector, connectedConnector);
                connectedConnector.Connections.Remove(connector);
                connectedConnector.UpdateFilled();
            }
        }

        Destroy(nodeLogic.gameObject);
    }

    public NodeLogic GetNodeById(int nodeId)
    {
        _spawnedNodes.TryGetValue(nodeId, out var node);
        return node;
    }

    public IEnumerable<KeyValuePair<int, NodeLogic>> GetAllNodes()
    {
        return _spawnedNodes;
    }

    public void ClearAllNodes()
    {
        foreach (var node in _spawnedNodes.Values.ToList())
        {
            if (node != null) DeleteNode(node);
        }
        _spawnedNodes.Clear();
        _nextNodeId = 1;
    }
}