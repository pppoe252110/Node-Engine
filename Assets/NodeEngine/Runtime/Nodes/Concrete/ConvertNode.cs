using System;

[NodePath("Conversion/Convert")]
public class ConvertNode : BaseNode, IConnectionListener
{
    [NodePort("Input", true)] public object input;
    [NodePort("TargetType", true)] public Type targetType;
    [NodePort("Result", false)] public object result;

    public void OnConnected(Connector myConnector, Connector otherConnector)
    {
        if (myConnector.PortName == "TargetType" && otherConnector.Node is TypeVariableNode typeNode)
            UpdateOutputType(typeNode.SelectedType);
    }

    public void OnDisconnected(Connector myConnector, Connector otherConnector)
    {
        if (myConnector.PortName == "TargetType")
            UpdateOutputType(typeof(object));
    }

    public void UpdateOutputType(Type newType)
    {
        Type resolvedType = newType ?? typeof(object);
        var resultConnector = LogicView?.OutputConnectors.Find(c => c.PortName == "Result");
        if (resultConnector != null)
            TypeChangeService.TryChangeConnectorType(resultConnector, resolvedType);
        else
        {
            var portInfo = Ports.Find(p => p.Name == "Result");
            if (portInfo != null) portInfo.ValueType = resolvedType;
        }
    }

    public override Func<GraphContext, int> Compile()
    {
        int inId = GetInputId("Input");
        int typeId = GetInputId("TargetType");
        int outId = GetOutputId("Result");

        return (ctx) =>
        {
            object val = ctx.Memory[inId];
            Type t = ctx.Memory[typeId] as Type;

            if (val != null && t != null)
            {
                try { ctx.Memory[outId] = Convert.ChangeType(val, t); }
                catch { ctx.Memory[outId] = null; }
            }
            else ctx.Memory[outId] = null;

            return -1;
        };
    }
}