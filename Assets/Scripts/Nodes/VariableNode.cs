using System.Drawing;
using UnityEngine;

public enum VariableType
{
    Int,
    Float,
    String,
    Vector3
}

public class VariableNode : NodeBase
{
    [SerializeField] private VariableDatabase _variableDatabase; // Assigned in NodesDatabase
    [SerializeField] private VariableType _variableType; // Set in editor

    public VariableDatabase VariableDatabase => _variableDatabase;
    public VariableType VariableType => _variableType;

    // Reference to the spawned UI element (set by NodeLogic)
    public VariableUIElement UIElement { get; set; }

    // Output the value via a connector (pulls from the spawned UI)
    [NodeValue("Value", typeof(object), KnownColor.Gray)]
    public void Output(ConnectorValueObject value)
    {
        // Use the UI element reference set by NodeLogic
        value.SetValue(UIElement?.GetValue());
    }

    public override void Setup()
    {
        // No input fields/connectors—input is via spawned UI
        outputFields = new()
        {
            new NodeField<ConnectorValueObject>(false).SetFunc(Output).ProvideDefaultValue(new ConnectorValueObject(null))
        };
    }
}