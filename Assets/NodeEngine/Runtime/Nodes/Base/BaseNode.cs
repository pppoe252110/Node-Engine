using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VContainer;
using NodeEngine.Compilation;

public abstract class BaseNode
{
    // --- UI & Editor Properties ---
    public string NodeName { get; protected set; }
    public Sprite NodeSprite { get; protected set; }
    public string NodeId { get; protected set; }
    public int RuntimeId { get; internal set; } = -1;
    public NodeLogic LogicView { get; protected set; }

    // Port definition
    public List<NodePortInfo> Ports { get; private set; }

    // Dynamic type forwarding
    protected Dictionary<string, List<string>> _dynamicTypes = new();

    // Services
    private TypeChangeService _typeChangeService;

    public BaseNode()
    {
        DiscoverPorts();
    }

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

    // --- Port Discovery ---
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
                portType = parameters.Length > 0 ? parameters[0].ParameterType : typeof(void);
            }

            portsList.Add(new NodePortInfo
            {
                Name = attr.Name,
                IsInput = attr.IsInput,
                IsFlow = attr.IsFlow,
                ValueType = portType,
                Order = attr.Order
            });
        }

        Ports = portsList
            .OrderBy(p => !p.IsInput)
            .ThenBy(p => p.Order)
            .ToList();
    }

    // Helper to find port index by name
    public bool TryGetPortIndex(string name, bool isInput, out int index)
    {
        index = -1;
        for (int i = 0; i < Ports.Count; i++)
        {
            var p = Ports[i];
            if (p.Name == name && p.IsInput == isInput)
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    // --- Compilation ---
    public abstract Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context);

    // --- Dynamic Type Binding ---
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
            if (otherConnector.Node is IVariableNode varNode && varNode.ValueType == typeof(Type))
            {
                Type selectedType = varNode.GetUntypedValue() as Type;
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

    // --- NodePortInfo nested class ---
    public class NodePortInfo
    {
        public string Name;
        public bool IsInput;
        public bool IsFlow;
        public Type ValueType;
        public int Order;
    }
}