using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniMediator.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class NodeSpawnerService : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NodeLogic _nodeLogicPrefab;
    [SerializeField] private Connector _leftConnectorPrefab;
    [SerializeField] private Connector _rightConnectorPrefab;
    [SerializeField] private ConnectorColorDatabase _colorDatabase;
    [SerializeField] private VariableDatabase _variableDatabase;

    private Dictionary<string, NodeLogic> _spawnedNodes = new();

    private IObjectResolver _objectResolver;
    private IMediator _mediator;
    private ConnectionManager _connectionManager;

    [Inject]
    public void Construct(IObjectResolver objectResolver, IMediator mediator, ConnectionManager connectionManager)
    {
        _objectResolver = objectResolver;
        _mediator = mediator;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Spawns a visual node for the given node instance.
    /// </summary>
    public NodeLogic SpawnNode(BaseNode nodeInstance, Vector2 position, string nodeId = null)
    {
        if (nodeInstance == null) return null;

        nodeId ??= Guid.NewGuid().ToString();

        var nodeLogic = _objectResolver.Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;

        nodeLogic.SetNodeBase(nodeInstance, nodeId);
        SetupNodeVisuals(nodeLogic, nodeInstance);

        _spawnedNodes[nodeId] = nodeLogic;

        _mediator.Publish(new MarkGraphDirtyNotification());
        return nodeLogic;
    }

    /// <summary>
    /// Deletes a node and all its connections.
    /// </summary>
    public void DeleteNode(NodeLogic nodeLogic)
    {
        if (nodeLogic == null) return;

        // Disconnect all connectors on this node
        var allConnectors = nodeLogic.InputConnectors.Concat(nodeLogic.OutputConnectors).ToList();
        foreach (var connector in allConnectors)
        {
            // Use a copy of the connections list because Disconnect modifies it
            foreach (var other in connector.Connections.ToArray())
            {
                _connectionManager.Disconnect(connector, other);
            }
        }

        // Remove from dictionary
        var nodeId = _spawnedNodes.FirstOrDefault(x => x.Value == nodeLogic).Key;
        if (nodeId != null) _spawnedNodes.Remove(nodeId);

        Destroy(nodeLogic.gameObject);
        _mediator.Publish(new MarkGraphDirtyNotification());
    }

    /// <summary>
    /// Returns all currently spawned node views.
    /// </summary>
    public IEnumerable<NodeLogic> GetAllNodes() => _spawnedNodes.Values;

    #region Private Setup Methods

    private void SetupNodeVisuals(NodeLogic nodeLogic, BaseNode node)
    {
        nodeLogic.NodeNameText.text = node.NodeName;
        nodeLogic.NodeTypeText.text = GetNodeTypeFromPath(node);
        nodeLogic.NodeIcon.sprite = node.NodeSprite;
        nodeLogic.NodeIcon.color = node.NodeSprite ? Color.white : Color.clear;

        GenerateConnectors(nodeLogic, node);

        // Create variable/converter UI if needed
        if (node is TypeVariableNode converterNode)
            nodeLogic.UIManager.CreateConverterUI(converterNode, nodeLogic.BackgroundImage);
        else if (node is VariableNode varNode)
            nodeLogic.UIManager.CreateVariableUI(varNode, nodeLogic.BackgroundImage);

        // Adjust node size based on port count
        int inputCount = node.Ports.Count(p => p.IsInput);
        int outputCount = node.Ports.Count(p => !p.IsInput);
        float height = 57 + Mathf.Max(inputCount, outputCount) * 25;
        nodeLogic.BackgroundImage.rectTransform.sizeDelta =
            new Vector2(nodeLogic.BackgroundImage.rectTransform.sizeDelta.x, height);

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

        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeLogic.LeftConnectorsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodeLogic.RightConnectorsParent);
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

    #endregion
}