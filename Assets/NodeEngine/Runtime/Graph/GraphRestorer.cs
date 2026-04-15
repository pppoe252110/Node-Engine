using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace NodeEngine.GraphPersistence
{
    public class GraphRestorer
    {
        private readonly INodeFactory _nodeFactory;
        private readonly NodeSpawnerService _nodeSpawner;
        private readonly ConnectionService _connectionService;
        private readonly GraphSerializer _serializer;
        private readonly NodesDatabase _nodesDatabase;
        private readonly NodeRunner _nodeRunner;

        [Inject]
        public GraphRestorer(
            INodeFactory nodeFactory,
            NodeSpawnerService nodeSpawner,
            ConnectionService connectionService,
            GraphSerializer serializer,
            NodesDatabase nodesDatabase,
            NodeRunner nodeRunner)
        {
            _nodeFactory = nodeFactory;
            _nodeSpawner = nodeSpawner;
            _connectionService = connectionService;
            _serializer = serializer;
            _nodesDatabase = nodesDatabase;
            _nodeRunner = nodeRunner;
        }

        public void Restore(GraphSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            Debug.Log("[GraphRestorer] Starting restore...");

            var nodeLookup = new Dictionary<string, BaseNode>();

            // Step 1: Create and spawn all nodes
            foreach (var nodeData in snapshot.nodes)
            {
                Type nodeType = Type.GetType(nodeData.nodeType);
                if (nodeType == null)
                {
                    Debug.LogWarning($"[GraphRestorer] Type not found: {nodeData.nodeType}");
                    continue;
                }

                BaseNode nodeInstance;
                try
                {
                    nodeInstance = _nodeFactory.CreateNode(nodeType);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GraphRestorer] Failed to create instance of {nodeType.Name}: {ex.Message}");
                    continue;
                }

                ApplyNodeMetadata(nodeInstance);

                // Restore variable value (existing)
                if (nodeInstance is IVariableNode varNode && !string.IsNullOrEmpty(nodeData.serializedValue))
                {
                    _serializer.DeserializeVariable(varNode, nodeData.serializedValue);
                }

                else if (nodeInstance is IGraphSerializable serializable && !string.IsNullOrEmpty(nodeData.serializedValue))
                {
                    serializable.DeserializeCustomData(nodeData.serializedValue);
                }

                if (nodeInstance is SubgraphNode subgraphNode)
                {
                    // Force initialization of definition from loaded ID
                    subgraphNode.ResolveDefinition();
                }

                Vector2 position = nodeData.position;
                var nodeLogic = _nodeSpawner.SpawnNode(nodeInstance, position, nodeData.nodeId);

                if (nodeLogic != null)
                    nodeLookup[nodeData.nodeId] = nodeInstance;
                else
                    Debug.LogError($"[GraphRestorer] SpawnNode returned null for {nodeData.nodeId}");
            }

            // Step 2: Recreate connections
            foreach (var connData in snapshot.connections)
            {
                if (!nodeLookup.TryGetValue(connData.sourceNodeId, out BaseNode sourceNode) ||
                    !nodeLookup.TryGetValue(connData.targetNodeId, out BaseNode targetNode))
                {
                    Debug.LogWarning($"[GraphRestorer] Source or target node not found: {connData.sourceNodeId} -> {connData.targetNodeId}");
                    continue;
                }

                Connector sourceConn = FindConnector(sourceNode, connData.sourcePortName, isOutput: true);
                Connector targetConn = FindConnector(targetNode, connData.targetPortName, isOutput: false);

                if (sourceConn == null || targetConn == null)
                {
                    Debug.LogWarning($"[GraphRestorer] Connector not found: {connData.sourcePortName} or {connData.targetPortName}");
                    continue;
                }

                _connectionService.CreateConnection(sourceConn, targetConn);
            }

            _nodeRunner?.MarkDirty();
            Debug.Log("[GraphRestorer] Restore complete.");
        }

        private void ApplyNodeMetadata(BaseNode node)
        {
            _nodesDatabase?.ApplyMetadata(node);
        }

        private Connector FindConnector(BaseNode node, string portName, bool isOutput)
        {
            var logic = node.LogicView;
            if (logic == null) return null;

            return isOutput
                ? logic.OutputConnectors.FirstOrDefault(c => c.PortName == portName)
                : logic.InputConnectors.FirstOrDefault(c => c.PortName == portName);
        }
    }
}