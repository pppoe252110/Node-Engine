using System.Collections.Generic;
using NodeEngine.GraphPersistence;
using UnityEngine;
using VContainer;

public class GraphSerializer
{
    private readonly PersistenceService _persistence;

    [Inject]
    public GraphSerializer(PersistenceService persistence)
    {
        _persistence = persistence;
    }

    public GraphSnapshot BuildSnapshot(
        IEnumerable<NodeLogic> nodes,
        IEnumerable<DataConnection> dataConnections,
        IEnumerable<FlowConnection> flowConnections)
    {
        var snapshot = new GraphSnapshot { version = "1.0" };

        foreach (var nodeLogic in nodes)
        {
            var node = nodeLogic.Node;
            var nodeData = new GraphSnapshot.NodeData
            {
                nodeId = node.NodeId,
                nodeType = node.GetType().AssemblyQualifiedName,
                position = nodeLogic.transform.localPosition
            };

            if (node is IVariableNode varNode)
            {
                nodeData.serializedValue = SerializeVariable(varNode);
            }
            else if (node is IGraphSerializable serializable)
            {
                nodeData.serializedValue = serializable.SerializeCustomData();
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

    public string SerializeToJson(GraphSnapshot snapshot, bool prettyPrint)
    {
        return JsonUtility.ToJson(snapshot, prettyPrint);
    }

    public GraphSnapshot DeserializeFromJson(string json)
    {
        return JsonUtility.FromJson<GraphSnapshot>(json);
    }

    public string SerializeVariable(IVariableNode varNode)
    {
        return _persistence.Serialize(varNode);
    }

    public void DeserializeVariable(IVariableNode varNode, string serialized)
    {
        if (string.IsNullOrEmpty(serialized)) return;
        _persistence.Deserialize(varNode, serialized);
    }
}