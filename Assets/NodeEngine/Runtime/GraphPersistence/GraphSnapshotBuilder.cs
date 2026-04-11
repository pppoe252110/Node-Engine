using System.Linq;
using VContainer;

namespace NodeEngine.GraphPersistence
{
    public class GraphSnapshotBuilder
    {
        private readonly GraphSerializer _serializer;
        private readonly NodeSpawnerService _nodeSpawner;
        private readonly ConnectionManager _connectionManager;

        [Inject]
        public GraphSnapshotBuilder(
            GraphSerializer serializer,
            NodeSpawnerService nodeSpawner,
            ConnectionManager connectionManager)
        {
            _serializer = serializer;
            _nodeSpawner = nodeSpawner;
            _connectionManager = connectionManager;
        }

        public GraphSnapshot BuildSnapshot()
        {
            var snapshot = new GraphSnapshot { version = "1.0" };

            var nodeLogics = _nodeSpawner.GetAllNodes().ToList();
            var dataConnections = _connectionManager.ActiveDataConnections;
            var flowConnections = _connectionManager.ActiveFlowConnections;

            foreach (var nodeLogic in nodeLogics)
            {
                var node = nodeLogic.Node;
                var nodeData = new GraphSnapshot.NodeData
                {
                    nodeId = node.NodeId,
                    nodeType = node.GetType().AssemblyQualifiedName,
                    position = nodeLogic.transform.localPosition,
                    nodeName = node.NodeName
                };

                if (node is IVariableNode varNode)
                {
                    nodeData.serializedValue = _serializer.SerializeVariable(varNode);
                }

                snapshot.nodes.Add(nodeData);
            }

            foreach (var conn in dataConnections)
            {
                snapshot.connections.Add(new GraphSnapshot.ConnectionData
                {
                    sourceNodeId = conn.SourceNode.NodeId,
                    sourcePortName = conn.OutputPortName,
                    targetNodeId = conn.TargetNode.NodeId,
                    targetPortName = conn.InputPortName,
                    isFlow = false
                });
            }

            foreach (var conn in flowConnections)
            {
                snapshot.connections.Add(new GraphSnapshot.ConnectionData
                {
                    sourceNodeId = conn.SourceNode.NodeId,
                    sourcePortName = conn.SourcePortName,
                    targetNodeId = conn.TargetNode.NodeId,
                    targetPortName = conn.TargetPortName,
                    isFlow = true
                });
            }

            return snapshot;
        }
    }
}