using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class NodeSpawnerService : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private ConnectorColorDatabase _colorDatabase;
    [SerializeField] private VariableDatabase _variableDatabase;

    private Dictionary<string, NodeLogic> _spawnedNodes = new();
    public static NodeSpawnerService Instance { get; private set; }

    private void Awake() => Instance = this;

    public NodeLogic SpawnNode(BaseNode nodeInstance, Vector2 position, string nodeId = null)
    {
        if (nodeInstance == null) return null;

        nodeId ??= Guid.NewGuid().ToString();

        var nodeLogic = Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;

        // Bind the data (no UI creation yet)
        nodeLogic.SetNodeBase(nodeInstance, nodeId);

        // Now create all UI elements (the moved logic)
        SetupNodeVisuals(nodeLogic, nodeInstance);

        _spawnedNodes[nodeId] = nodeLogic;
        return nodeLogic;
    }

    private void SetupNodeVisuals(NodeLogic nodeLogic, BaseNode node)
    {
        // 1. Basic visuals
        nodeLogic.NodeNameText.text = node.NodeName;
        nodeLogic.NodeTypeText.text = GetNodeTypeFromPath(node);
        nodeLogic.NodeIcon.sprite = node.NodeSprite;
        nodeLogic.NodeIcon.color = node.NodeSprite ? Color.white : Color.clear;

        // 2. ALWAYS generate connectors from NodePort attributes
        GenerateConnectors(nodeLogic, node);

        // 3. If it's a variable node, add the custom UI element (input field / dropdown)
        if (node is TypeVariableNode converterNode)
        {
            nodeLogic.UIManager.CreateConverterUI(converterNode, nodeLogic.BackgroundImage);
        }
        else if (node is VariableNode varNode)
        {
            nodeLogic.UIManager.CreateVariableUI(varNode, nodeLogic.BackgroundImage);
        }

        // 4. Resize node based on port count
        int inputCount = node.Ports.Count(p => p.IsInput);
        int outputCount = node.Ports.Count(p => !p.IsInput);
        float height = 57 + (Mathf.Max(inputCount, outputCount)) * 25;

        nodeLogic.BackgroundImage.rectTransform.sizeDelta = new Vector2(
            nodeLogic.BackgroundImage.rectTransform.sizeDelta.x, height);

        nodeLogic.BackgroundImage.material = new Material(nodeLogic.BackgroundImage.material);
        nodeLogic.RecalculateMaterial();
    }

    private void GenerateConnectors(NodeLogic nodeLogic, BaseNode node)
    {
        foreach (var port in node.Ports)
        {
            var prefab = port.IsInput ? _leftConnectorPrefab : _rightConnectorPrefab;
            var parent = port.IsInput ? nodeLogic.LeftConnectorsParent : nodeLogic.RightConnectorsParent;

            var connector = Instantiate(prefab, parent);
            connector.Setup(port.Name, port.ValueType, port.IsInput, port.IsFlow, node);
            connector.SetColorDatabase(_colorDatabase);

            if (port.IsInput)
                nodeLogic.InputConnectors.Add(connector);
            else
                nodeLogic.OutputConnectors.Add(connector);
        }
    }

    private string GetNodeTypeFromPath(BaseNode node)
    {
        var pathAttr = node.GetType().GetCustomAttribute<NodePathAttribute>();
        if (pathAttr != null && !string.IsNullOrEmpty(pathAttr.Path))
        {
            int slash = pathAttr.Path.IndexOf('/');
            return slash >= 0 ? pathAttr.Path.Substring(0, slash) : pathAttr.Path;
        }
        return node.GetType().Name.Replace("Node", "");
    }

    public void DeleteNode(NodeLogic nodeLogic)
    {
        if (nodeLogic == null) return;

        foreach (var connector in nodeLogic.InputConnectors.Concat(nodeLogic.OutputConnectors))
        {
            foreach (var other in connector.Connections.ToArray())
            {
                ConnectionManager.Instance.Disconnect(connector, other);
            }
        }

        var nodeId = _spawnedNodes.FirstOrDefault(x => x.Value == nodeLogic).Key;
        if (nodeId != null) _spawnedNodes.Remove(nodeId);

        Destroy(nodeLogic.gameObject);
        NodeRunner.Instance?.MarkDirty();
    }

    public IEnumerable<NodeLogic> GetAllNodes() => _spawnedNodes.Values;
}