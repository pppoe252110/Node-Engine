// ===== Assets/NodeEngine/Runtime/Nodes/Base/BaseNode.cs =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public struct DataLink
{
    public BaseNode SourceNode;
    public string OutputPortName;
}

public abstract class BaseNode : IConnectionListener
{
    // --- UI & Editor Properties ---
    public string NodeName { get; protected set; }
    public Sprite NodeSprite { get; protected set; }
    public string NodeId { get; protected set; }
    public NodeLogic LogicView { get; protected set; }

    public int[] InputMemoryIndices;
    public int[] OutputMemoryIndices;
    public List<NodePortInfo> Ports = new List<NodePortInfo>();

    // --- Cached API Maps ---
    protected Dictionary<string, int> _inMap = new();
    protected Dictionary<string, int> _outMap = new();
    protected Dictionary<string, int> _flowMap = new();
    protected Dictionary<string, List<string>> _dynamicTypes = new();

    public BaseNode() => DiscoverPorts();

    public virtual void Initialize(NodeLogic logic, string guid)
    {
        LogicView = logic;
        NodeId = guid;
    }

    public void SetName(string name) => NodeName = name;
    public void SetIcon(Sprite icon) => NodeSprite = icon;

    private void DiscoverPorts()
    {
        var portsList = new List<NodePortInfo>();
        var members = GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var member in members)
        {
            var attr = (NodePortAttribute)Attribute.GetCustomAttribute(member, typeof(NodePortAttribute));
            if (attr == null) continue;

            Type portType = typeof(void);
            if (member is FieldInfo f) portType = f.FieldType;
            else if (member is MethodInfo m)
            {
                var parameters = m.GetParameters();
                if (parameters.Length > 0) portType = parameters[0].ParameterType;
                else portType = typeof(void);
            }

            portsList.Add(new NodePortInfo
            {
                Name = attr.Name,
                IsInput = attr.IsInput,
                IsFlow = attr.IsFlow,
                ValueType = portType
            });
        }

        Ports = portsList.OrderBy(p => p.Name).ToList();
    }

    // Called by the compiler to map names to memory blocks
    public void SetMemoryIndices(int[] inputs, int[] outputs)
    {
        InputMemoryIndices = inputs;
        OutputMemoryIndices = outputs;

        int inIdx = 0, outIdx = 0;
        foreach (var port in Ports)
        {
            if (port.IsFlow) continue;
            if (port.IsInput) _inMap[port.Name] = inputs[inIdx++];
            else _outMap[port.Name] = outputs[outIdx++];
        }
    }

    public virtual void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        _flowMap = flowTargets;
    }

    public abstract Func<GraphContext, int> Compile();

    // ==========================================
    // EASY COMPILER API
    // ==========================================

    protected T GetInput<T>(GraphContext ctx, string portName, T fallback = default)
    {
        if (_inMap.TryGetValue(portName, out int memIdx) && memIdx >= 0 && memIdx < ctx.Memory.Length)
        {
            var val = ctx.Memory[memIdx];
            if (val is T castedVal) return castedVal;

            try
            {
                if (val is IConvertible) return (T)Convert.ChangeType(val, typeof(T));
            }
            catch { }
        }
        return fallback;
    }

    protected object GetInput(GraphContext ctx, string portName)
    {
        if (_inMap.TryGetValue(portName, out int memIdx) && memIdx >= 0 && memIdx < ctx.Memory.Length)
            return ctx.Memory[memIdx];
        return null;
    }

    protected void SetOutput(GraphContext ctx, string portName, object value)
    {
        if (_outMap.TryGetValue(portName, out int memIdx) && memIdx >= 0 && memIdx < ctx.Memory.Length)
            ctx.Memory[memIdx] = value;
    }

    protected int GetFlow(string portName)
    {
        return _flowMap.TryGetValue(portName, out int idx) ? idx : -1;
    }

    // ==========================================
    // 1. COMPILATION PHASE LOOKUPS (Run Once)
    // ==========================================

    protected int GetInputId(string portName) => _inMap.TryGetValue(portName, out int idx) ? idx : -1;
    protected int GetOutputId(string portName) => _outMap.TryGetValue(portName, out int idx) ? idx : -1;
    protected int GetFlowId(string portName) => _flowMap.TryGetValue(portName, out int idx) ? idx : -1;


    // ==========================================
    // 2. EXECUTION PHASE HELPERS (Blazing Fast)
    // ==========================================

    protected T Read<T>(GraphContext ctx, int memoryId, T fallback = default)
    {
        if (memoryId >= 0 && memoryId < ctx.Memory.Length)
        {
            var val = ctx.Memory[memoryId];
            if (val is T castedVal) return castedVal;

            try
            {
                if (val is IConvertible) return (T)Convert.ChangeType(val, typeof(T));
            }
            catch { }
        }
        return fallback;
    }

    protected void Write(GraphContext ctx, int memoryId, object value)
    {
        if (memoryId >= 0 && memoryId < ctx.Memory.Length)
            ctx.Memory[memoryId] = value;
    }

    // ==========================================
    // DYNAMIC TYPING SETUP
    // ==========================================

    protected void BindDynamicType(string sourcePort, string targetPort)
    {
        if (!_dynamicTypes.ContainsKey(sourcePort))
            _dynamicTypes[sourcePort] = new List<string>();

        if (!_dynamicTypes[sourcePort].Contains(targetPort))
            _dynamicTypes[sourcePort].Add(targetPort);
    }

    public virtual void OnConnected(Connector myConnector, Connector otherConnector)
    {
        if (_dynamicTypes.TryGetValue(myConnector.PortName, out var targets))
        {
            if (otherConnector.Node is TypeVariableNode typeNode)
            {
                foreach (var target in targets)
                    UpdatePortType(target, typeNode.SelectedType);
            }
        }
    }

    public virtual void OnDisconnected(Connector myConnector, Connector otherConnector)
    {
        if (_dynamicTypes.TryGetValue(myConnector.PortName, out var targets))
        {
            foreach (var target in targets)
                UpdatePortType(target, typeof(object));
        }
    }

    protected void UpdatePortType(string portName, Type newType)
    {
        Type resolvedType = newType ?? typeof(object);
        var connector = LogicView?.InputConnectors.Find(c => c.PortName == portName) ??
                        LogicView?.OutputConnectors.Find(c => c.PortName == portName);

        if (connector != null)
        {
            TypeChangeService.TryChangeConnectorType(connector, resolvedType);
        }
        else
        {
            var portInfo = Ports.Find(p => p.Name == portName);
            if (portInfo != null) portInfo.ValueType = resolvedType;
        }
    }

    public class NodePortInfo
    {
        public string Name;
        public bool IsInput;
        public bool IsFlow;
        public Type ValueType;
    }
}