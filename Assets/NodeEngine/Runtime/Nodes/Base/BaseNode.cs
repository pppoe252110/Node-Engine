using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;

public abstract class BaseNode : IConnectionListener
{
    // --- UI & Editor Properties ---
    public string NodeName { get; protected set; }
    public Sprite NodeSprite { get; protected set; }
    public string NodeId { get; protected set; }
    public int RuntimeId { get; internal set; } = -1;
    public NodeLogic LogicView { get; protected set; }

    public int[] InputMemoryIndices;
    public int[] OutputMemoryIndices;
    public List<NodePortInfo> Ports = new List<NodePortInfo>();

    // --- Cached maps filled by the compiler ---
    protected Dictionary<string, int> _inMap = new();
    protected Dictionary<string, int> _outMap = new();
    protected Dictionary<string, int> _flowMap = new();

    // --- Dynamic type forwarding ---
    protected Dictionary<string, List<string>> _dynamicTypes = new();

    private TypeChangeService _typeChangeService;

    public BaseNode() => DiscoverPorts();

    [Inject]
    public void Construct(TypeChangeService typeChangeService)
    {
        _typeChangeService = typeChangeService;
    }

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

    public virtual void SetFlowTargets(Dictionary<string, int> flowTargets)
    {
        AssignFlowIndices(flowTargets);
    }

    public abstract Func<GraphContext, ExecutionResult> Compile();

    // ==========================================
    // COMPILATION PHASE LOOKUPS (Run Once)
    // ==========================================
    protected int GetInputId(string portName) => _inMap.TryGetValue(portName, out int idx) ? idx : -1;
    protected int GetOutputId(string portName) => _outMap.TryGetValue(portName, out int idx) ? idx : -1;
    protected int GetFlowId(string portName) => _flowMap.TryGetValue(portName, out int idx) ? idx : -1;

    // ==========================================
    // EXECUTION PHASE HELPERS (Fast path)
    // ==========================================
    protected T Read<T>(GraphContext ctx, int memoryId, T fallback = default)
    {
        if (memoryId >= 0 && memoryId < ctx.Memory.Length)
        {
            var val = ctx.Memory[memoryId];
            if (val is T castedVal) return castedVal;
            try { if (val is IConvertible) return (T)Convert.ChangeType(val, typeof(T)); } catch { }
        }
        return fallback;
    }

    protected void Write(GraphContext ctx, int memoryId, object value)
    {
        if (memoryId >= 0 && memoryId < ctx.Memory.Length)
            ctx.Memory[memoryId] = value;
    }

    // ==========================================
    // DYNAMIC TYPING
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
        if (_dynamicTypes == null) return;

        if (_dynamicTypes.TryGetValue(myConnector.PortName, out var targets))
        {
            if (otherConnector.Node is TypeVariableNode typeNode)
            {
                Type selectedType = typeNode.SelectedType; // May be null
                foreach (var target in targets)
                    UpdatePortType(target, selectedType);
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
        var connector = LogicView?.OutputConnectors.Find(c => c.PortName == portName);
        if (connector != null)
        {
            _typeChangeService.TryChangeConnectorType(connector, resolvedType);
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