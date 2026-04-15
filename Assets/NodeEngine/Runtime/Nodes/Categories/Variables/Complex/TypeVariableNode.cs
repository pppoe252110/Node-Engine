using System;
using System.Linq;

[NodePath("Variables/Type")]
public class TypeVariableNode : VariableNode<Type>
{
    public void ChangeSelectedType(Type newType)
    {
        SetValue(newType);
        PropagateSelectedType(newType);
    }

    private void PropagateSelectedType(Type newType)
    {
        var outputConnector = LogicView?.OutputConnectors.FirstOrDefault(c => c.PortName == "Value");
        if (outputConnector == null) return;

        foreach (var targetConnector in outputConnector.Connections)
        {
            BaseNode targetNode = targetConnector.Node;
            if (targetNode.TryGetDynamicBindingTargets(targetConnector.PortName, out var boundOutputs))
            {
                foreach (string outputPortName in boundOutputs)
                {
                    targetNode.UpdatePortType(outputPortName, newType);
                }
            }
        }
    }
}