using System;
using System.Collections.Generic;
using System.Linq;
using UniMediator.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class NodeSpawnerService : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NodeLogic _nodeLogicPrefab;

    private Dictionary<string, NodeLogic> _spawnedNodes = new();
    private IObjectResolver _objectResolver;
    private IMediator _mediator;
    private ConnectionService _connectionService;

    [Inject]
    public void Construct(IObjectResolver objectResolver, IMediator mediator, ConnectionService connectionService)
    {
        _objectResolver = objectResolver;
        _mediator = mediator;
        _connectionService = connectionService;
    }

    public NodeLogic SpawnNode(BaseNode nodeInstance, Vector2 position, string nodeId = null)
    {
        if (nodeInstance == null) return null;

        nodeId ??= Guid.NewGuid().ToString();

        var nodeLogic = _objectResolver.Instantiate(_nodeLogicPrefab, UIZoomPan.NodesParent);
        nodeLogic.transform.localPosition = position;
        nodeLogic.SetNodeBase(nodeInstance, nodeId);

        // Use the UIManager from the spawned NodeLogic instance
        nodeLogic.UIManager.SetupNodeVisuals(nodeLogic, nodeInstance);

        _spawnedNodes[nodeId] = nodeLogic;
        _mediator.Publish(new MarkGraphDirtyNotification());
        return nodeLogic;
    }

    public void DeleteNode(NodeLogic nodeLogic)
    {
        if (nodeLogic == null) return;

        var allConnectors = nodeLogic.InputConnectors.Concat(nodeLogic.OutputConnectors).ToList();
        foreach (var connector in allConnectors)
        {
            foreach (var other in connector.Connections.ToArray())
            {
                var source = connector.IsInput ? other : connector;
                var target = connector.IsInput ? connector : other;
                _connectionService.Disconnect(source, target);
            }
        }

        var nodeId = _spawnedNodes.FirstOrDefault(x => x.Value == nodeLogic).Key;
        if (nodeId != null) _spawnedNodes.Remove(nodeId);

        Destroy(nodeLogic.gameObject);
        _mediator.Publish(new MarkGraphDirtyNotification());
    }

    public IEnumerable<NodeLogic> GetAllNodes() => _spawnedNodes.Values;
}