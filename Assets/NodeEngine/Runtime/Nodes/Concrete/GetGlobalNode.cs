using System;
using System.Collections.Generic;

[NodePath("Variables/Get Global")]
public class GetGlobalNode : BaseNode, IConnectionListener
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Out", false, true)] public void Exit() { }
    [NodePort("Name", true)] public string variableName;
    [NodePort("Type", true)] public Type expectedType;
    [NodePort("Value", false)] public object value;

    [NonSerialized] public int NextExitIndex = -1;

    public override void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        NextExitIndex = flowTargets.TryGetValue("Out", out var idx) ? idx : -1;
    }

    public override Func<GraphContext, int> Compile()
    {
        int nameId = GetInputId("Name");
        int typeId = GetInputId("Type");
        int outId = GetOutputId("Value");

        return (ctx) =>
        {
            string name = Read<string>(ctx, nameId);
            object rawValue = SetGlobalNode.GetGlobalValue(name);
            Type targetType = ctx.Memory[typeId] as Type;

            if (rawValue != null && targetType != null && targetType.IsInstanceOfType(rawValue))
                Write(ctx, outId, rawValue);
            else
                Write(ctx, outId, null);

            return NextExitIndex;
        };
    }

    public void OnConnected(Connector myConnector, Connector otherConnector)
    {
        if (myConnector.PortName == "Type" && otherConnector.Node is TypeVariableNode typeNode)
            UpdateOutputType(typeNode.SelectedType);
    }

    public void OnDisconnected(Connector myConnector, Connector otherConnector)
    {
        if (myConnector.PortName == "Type")
            UpdateOutputType(typeof(object));
    }

    private void UpdateOutputType(Type newType)
    {
        Type resolvedType = newType ?? typeof(object);
        var outputConnector = LogicView?.OutputConnectors.Find(c => c.PortName == "Value");
        if (outputConnector != null)
            TypeChangeService.TryChangeConnectorType(outputConnector, resolvedType);
        else
        {
            var portInfo = Ports.Find(p => p.Name == "Value" && !p.IsInput);
            if (portInfo != null) portInfo.ValueType = resolvedType;
        }
    }
}