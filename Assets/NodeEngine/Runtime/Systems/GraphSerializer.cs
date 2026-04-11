using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphSerializer
{
    public SerializableGraph Serialize(
        IEnumerable<NodeLogic> nodes,
        IEnumerable<DataConnection> dataConnections,
        IEnumerable<FlowConnection> flowConnections)
    {
        var graph = new SerializableGraph { version = "1.0" };

        foreach (var nodeLogic in nodes)
        {
            var node = nodeLogic.Node;
            var nodeData = new SerializableGraph.NodeData
            {
                nodeId = node.NodeId,
                nodeType = node.GetType().AssemblyQualifiedName,
                position = nodeLogic.transform.localPosition,
                nodeName = node.NodeName
            };

            if (node is VariableNode varNode)
            {
                nodeData.variableType = (int)varNode.VariableType;
                nodeData.serializedValue = SerializeVariableValue(varNode);
            }

            graph.nodes.Add(nodeData);
        }

        foreach (var conn in dataConnections)
        {
            graph.connections.Add(new SerializableGraph.ConnectionData
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
            graph.connections.Add(new SerializableGraph.ConnectionData
            {
                sourceNodeId = conn.SourceNode.NodeId,
                sourcePortName = conn.SourcePortName,
                targetNodeId = conn.TargetNode.NodeId,
                targetPortName = conn.TargetPortName,
                isFlow = true
            });
        }

        return graph;
    }

    public string SerializeToJson(SerializableGraph graph, bool prettyPrint)
    {
        return JsonUtility.ToJson(graph, prettyPrint);
    }

    public SerializableGraph DeserializeFromJson(string json)
    {
        return JsonUtility.FromJson<SerializableGraph>(json);
    }

    private string SerializeVariableValue(VariableNode varNode)
    {
        object value = varNode.GetValue();
        if (value == null) return "";

        return varNode.VariableType switch
        {
            VariableType.Single => ((float)value).ToString(System.Globalization.CultureInfo.InvariantCulture),
            VariableType.Vector2 => JsonUtility.ToJson((Vector2)value),
            VariableType.Vector3 => JsonUtility.ToJson((Vector3)value),
            VariableType.Color => JsonUtility.ToJson((Color)value),
            VariableType.Type => ((Type)value).AssemblyQualifiedName,
            VariableType.ComparisonOperation => ((ComparisonOperation)value).ToString(),
            _ => value.ToString()
        };
    }

    public void DeserializeVariableValue(VariableNode varNode, string serialized)
    {
        if (string.IsNullOrEmpty(serialized)) return;
        try
        {
            object value = varNode.VariableType switch
            {
                VariableType.Single => float.Parse(serialized, System.Globalization.CultureInfo.InvariantCulture),
                VariableType.Int => int.Parse(serialized),
                VariableType.Bool => bool.Parse(serialized),
                VariableType.String => serialized,
                VariableType.Vector3 => JsonUtility.FromJson<Vector3>(serialized),
                VariableType.Vector2 => JsonUtility.FromJson<Vector2>(serialized),
                VariableType.Color => JsonUtility.FromJson<Color>(serialized),
                VariableType.Type => Type.GetType(serialized) ?? typeof(object),
                VariableType.ComparisonOperation => Enum.Parse<ComparisonOperation>(serialized),
                _ => null
            };
            varNode.SetValue(value);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GraphSerializer] Failed to deserialize {varNode.VariableType}: {ex.Message}");
        }
    }
}

[Serializable]
public class SerializableGraph
{
    public string version;
    public List<NodeData> nodes = new List<NodeData>();
    public List<ConnectionData> connections = new List<ConnectionData>();

    [Serializable]
    public class NodeData
    {
        public string nodeId;
        public string nodeType;
        public string nodeName;
        public Vector2 position;
        public int variableType;
        public string serializedValue;
    }

    [Serializable]
    public class ConnectionData
    {
        public string sourceNodeId;
        public string sourcePortName;
        public string targetNodeId;
        public string targetPortName;
        public bool isFlow;
    }
}
