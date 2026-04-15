using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

[Serializable]
public class SubgraphDefinition
{
    [Serializable]
    public class PortDefinition
    {
        public string id;
        public string name;
        public string typeName;
        public bool isFlow;
        public string boundNodeId;  
        public string boundPortName;
    }

    [Serializable]
    public class SerializedNode
    {
        public string nodeId;
        public string nodeType;
        public Vector2 position;
        public string serializedValue;
    }

    [Serializable]
    public class SerializedConnection
    {
        public string sourceNodeId;
        public string sourcePortName;
        public string targetNodeId;
        public string targetPortName;
        public bool isFlow;
    }

    public string subgraphId = Guid.NewGuid().ToString();
    public string subgraphName = "New Subgraph";

    public List<SerializedNode> nodes = new();
    public List<SerializedConnection> connections = new();
    public List<PortDefinition> inputPorts = new();
    public List<PortDefinition> outputPorts = new();

    [NonSerialized] public SubgraphLibraryService Library;

    [NonSerialized] public NodeCompiler.CompiledGraph CompiledGraph;
    [NonSerialized] private Dictionary<string, SubgraphInputNodeBase> _inputNodes;
    [NonSerialized] private Dictionary<string, SubgraphOutputNodeBase> _outputNodes;
    [NonSerialized] private Dictionary<string, PortDefinition> _portIdToDef;

    public IEnumerable<PortDefinition> InputPorts => inputPorts;
    public IEnumerable<PortDefinition> OutputPorts => outputPorts;

    public void MarkDirty()
    {
        CompiledGraph = null;
        _inputNodes = null;
        _outputNodes = null;
    }

    // Add a new port
    public PortDefinition AddPort(string name, Type type, bool isInput, bool isFlow = false)
    {
        var port = new PortDefinition
        {
            id = Guid.NewGuid().ToString(),
            name = name,
            typeName = type.AssemblyQualifiedName,
            isFlow = isFlow
        };
        if (isInput) inputPorts.Add(port);
        else outputPorts.Add(port);
        MarkDirty();
        return port;
    }

    public void RemovePort(string portId)
    {
        inputPorts.RemoveAll(p => p.id == portId);
        outputPorts.RemoveAll(p => p.id == portId);
        MarkDirty();
    }

    /// <summary>Builds live node objects from serialized data.</summary>
    public List<BaseNode> InstantiateNodes(IObjectResolver resolver, PersistenceService persistence)
    {
        var nodeLookup = new Dictionary<string, BaseNode>();
        _inputNodes = new Dictionary<string, SubgraphInputNodeBase>();
        _outputNodes = new Dictionary<string, SubgraphOutputNodeBase>();
        _portIdToDef = inputPorts.Concat(outputPorts).ToDictionary(p => p.id);

        var nodeInstances = new List<BaseNode>();
        foreach (var sNode in nodes)
        {
            Type nodeType = Type.GetType(sNode.nodeType);
            if (nodeType == null) continue;

            var instance = (BaseNode)resolver.Resolve(nodeType);
            instance.Initialize(null, sNode.nodeId);

            // Deserialize variable value if applicable
            if (instance is IVariableNode varNode && !string.IsNullOrEmpty(sNode.serializedValue))
            {
                persistence.Deserialize(varNode, sNode.serializedValue);
            }

            nodeInstances.Add(instance);
            nodeLookup[sNode.nodeId] = instance;

            if (instance is SubgraphInputNodeBase input)
                _inputNodes[sNode.nodeId] = input;
            else if (instance is SubgraphOutputNodeBase output)
                _outputNodes[sNode.nodeId] = output;
        }

        var dataConns = new List<DataConnection>();
        var flowConns = new List<FlowConnection>();
        foreach (var sConn in connections)
        {
            if (!nodeLookup.TryGetValue(sConn.sourceNodeId, out var source) ||
                !nodeLookup.TryGetValue(sConn.targetNodeId, out var target))
                continue;

            if (sConn.isFlow)
                flowConns.Add(new FlowConnection { SourceNode = source, SourcePortName = sConn.sourcePortName, TargetNode = target, TargetPortName = sConn.targetPortName });
            else
                dataConns.Add(new DataConnection { SourceNode = source, OutputPortName = sConn.sourcePortName, TargetNode = target, InputPortName = sConn.targetPortName });
        }

        CompiledGraph = NodeCompiler.Compile(nodeInstances, dataConns, flowConns);
        return nodeInstances;
    }

    public void SaveFromLiveNodes(
        IEnumerable<NodeLogic> nodeLogics,
        IEnumerable<DataConnection> dataConns,
        IEnumerable<FlowConnection> flowConns)
    {
        nodes.Clear();
        connections.Clear();

        foreach (var logic in nodeLogics)
        {
            var node = logic.Node;
            nodes.Add(new SerializedNode
            {
                nodeId = node.NodeId,
                nodeType = node.GetType().AssemblyQualifiedName,
                position = logic.transform.localPosition,
                serializedValue = (node is IVariableNode vn) ? SerializeVariable(vn) : null
            });
        }

        foreach (var dc in dataConns)
        {
            connections.Add(new SerializedConnection
            {
                sourceNodeId = dc.SourceNode.NodeId,
                sourcePortName = dc.OutputPortName,
                targetNodeId = dc.TargetNode.NodeId,
                targetPortName = dc.InputPortName,
                isFlow = false
            });
        }
        foreach (var fc in flowConns)
        {
            connections.Add(new SerializedConnection
            {
                sourceNodeId = fc.SourceNode.NodeId,
                sourcePortName = fc.SourcePortName,
                targetNodeId = fc.TargetNode.NodeId,
                targetPortName = fc.TargetPortName,
                isFlow = true
            });
        }

        MarkDirty();
    }

    private string SerializeVariable(IVariableNode node)
    {
        // You'll need access to PersistenceService here.
        // Option A: Pass PersistenceService as a parameter.
        // Option B: Make this method internal and call it from the editor manager.
        // For simplicity, the editor manager already has _persistence; you can serialize outside.
        return "";
    }

    /// <summary>Returns a map from port ID (the port definition's unique id) to the input node instance.</summary>
    public IReadOnlyDictionary<string, SubgraphInputNodeBase> GetInputNodeMap()
    {
        if (_inputNodes == null) return new Dictionary<string, SubgraphInputNodeBase>();
        // Map from port ID -> node instance (via boundNodeId)
        var result = new Dictionary<string, SubgraphInputNodeBase>();
        foreach (var portDef in inputPorts)
        {
            if (!string.IsNullOrEmpty(portDef.boundNodeId) && _inputNodes.TryGetValue(portDef.boundNodeId, out var node))
                result[portDef.id] = node;
        }
        return result;
    }

    /// <summary>Returns a map from port ID to the output node instance.</summary>
    public IReadOnlyDictionary<string, SubgraphOutputNodeBase> GetOutputNodeMap()
    {
        if (_outputNodes == null) return new Dictionary<string, SubgraphOutputNodeBase>();
        var result = new Dictionary<string, SubgraphOutputNodeBase>();
        foreach (var portDef in outputPorts)
        {
            if (!string.IsNullOrEmpty(portDef.boundNodeId) && _outputNodes.TryGetValue(portDef.boundNodeId, out var node))
                result[portDef.id] = node;
        }
        return result;
    }

    /// <summary>Returns the port definition for a given port ID.</summary>
    public PortDefinition GetPortDefinition(string portId) => _portIdToDef?.GetValueOrDefault(portId);
}