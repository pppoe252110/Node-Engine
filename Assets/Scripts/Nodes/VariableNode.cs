using System;
using System.Drawing;
using UnityEngine;

public class VariableNode : NodeBase
{
    [SerializeField] private VariableType _variableType; // Set in editor

    public VariableType VariableType => _variableType;

    // Reference to the spawned UI element (set by NodeLogic)
    public VariableUIElement UIElement { get; set; }

    public override void Setup()
    {
        // No input fields/connectors—input is via spawned UI
        outputFields = new()
        {
            new VariableNodeField(_variableType).SetFunc(Output).ProvideDefaultValue(new ConnectorValueObject(null))
        };
    }

    [NodeValue("Value", typeof(object), KnownColor.Gray)]  // Type is overridden by VariableNodeField.GetValueType()
    public void Output(ConnectorValueObject value)
    {
        value.SetValue(UIElement?.GetValue());
    }
}
