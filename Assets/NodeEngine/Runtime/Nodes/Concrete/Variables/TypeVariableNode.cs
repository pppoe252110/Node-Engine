using System;
using UnityEngine;

[NodePath("Variables/Type")]
public class TypeVariableNode : VariableNode
{
    public override VariableType VariableType => VariableType.Type;
    public Type SelectedType => GetValue() as Type;

    public void ChangeSelectedType(Type newType)
    {
        Debug.Log($"TypeVariableNode.ChangeSelectedType: {newType?.Name}");
        SetValue(newType);

        var myConnector = LogicView?.OutputConnectors.Find(c => c.PortName == "Value");
        if (myConnector != null)
        {
            foreach (var conn in myConnector.Connections)
            {
                if (conn.Node is ConvertNode convertNode)
                    convertNode.UpdateOutputType(newType);
            }
        }
    }
}