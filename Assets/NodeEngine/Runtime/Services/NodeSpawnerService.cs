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
    private NodesDatabase _nodesDatabase;

    [Inject]
    public void Construct(IObjectResolver objectResolver, IMediator mediator, NodesDatabase nodesDatabase, ConnectionService connectionService)
    {
        _objectResolver = objectResolver;
        _mediator = mediator;
        _nodesDatabase = nodesDatabase;
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

    public NodeLogic DuplicateNode(NodeLogic original, Vector2 offset)
    {
        if (original?.Node == null) return null;

        var nodeType = original.Node.GetType();
        var newNodeInstance = _objectResolver.Resolve(nodeType) as BaseNode;
        _nodesDatabase?.ApplyMetadata(newNodeInstance);

        // Copy variable value if applicable
        if (original.Node is IVariableNode originalVar && newNodeInstance is IVariableNode newVar)
        {
            newVar.SetUntypedValue(originalVar.GetUntypedValue());
        }

        Vector2 newPos = (Vector2)original.transform.localPosition + offset;
        return SpawnNode(newNodeInstance, newPos);
    }

    /// <summary>
    /// Duplicates a set of nodes and recreates connections between them.
    /// Connections to nodes outside the selected set are not copied.
    /// </summary>
    public List<NodeLogic> DuplicateNodes(IEnumerable<NodeLogic> originals, Vector2 offset)
    {
        var originalList = originals.ToList();
        var originalToClone = new Dictionary<NodeLogic, NodeLogic>();
        var clones = new List<NodeLogic>();

        // Step 1: Create clones
        foreach (var original in originalList)
        {
            var clone = DuplicateNodeInternal(original, offset);
            if (clone != null)
            {
                originalToClone[original] = clone;
                clones.Add(clone);
            }
        }

        // Step 2: Copy connections where both endpoints are in the selected set
        var selectedOriginalNodes = new HashSet<BaseNode>(originalList.Select(l => l.Node));

        foreach (var original in originalList)
        {
            if (!originalToClone.TryGetValue(original, out var cloneLogic))
                continue;

            var originalNode = original.Node;
            var cloneNode = cloneLogic.Node;

            // Iterate over all output connectors (data + flow)
            foreach (var sourceConn in original.OutputConnectors)
            {
                foreach (var targetConn in sourceConn.Connections)
                {
                    var targetOriginalNode = targetConn.Node;
                    var targetOriginalLogic = targetOriginalNode.LogicView;

                    // Only duplicate if target was also selected
                    if (selectedOriginalNodes.Contains(targetOriginalNode) &&
                        originalToClone.TryGetValue(targetOriginalLogic, out var targetCloneLogic))
                    {
                        // Find matching connectors on clones
                        var sourceCloneConn = cloneLogic.OutputConnectors
                            .FirstOrDefault(c => c.PortName == sourceConn.PortName);
                        var targetCloneConn = targetCloneLogic.InputConnectors
                            .FirstOrDefault(c => c.PortName == targetConn.PortName);

                        if (sourceCloneConn != null && targetCloneConn != null)
                        {
                            _connectionService.CreateConnection(sourceCloneConn, targetCloneConn);
                        }
                    }
                }
            }
        }

        return clones;
    }

    // Internal helper that duplicates a single node without connection copying
    private NodeLogic DuplicateNodeInternal(NodeLogic original, Vector2 offset)
    {
        if (original?.Node == null) return null;

        var nodeType = original.Node.GetType();
        var newNodeInstance = _objectResolver.Resolve(nodeType) as BaseNode;
        _nodesDatabase?.ApplyMetadata(newNodeInstance);

        if (original.Node is IVariableNode originalVar && newNodeInstance is IVariableNode newVar)
        {
            newVar.SetUntypedValue(originalVar.GetUntypedValue());
        }

        Vector2 newPos = (Vector2)original.transform.localPosition + offset;
        return SpawnNode(newNodeInstance, newPos);
    }

    public IEnumerable<NodeLogic> GetAllNodes() => _spawnedNodes.Values;
}