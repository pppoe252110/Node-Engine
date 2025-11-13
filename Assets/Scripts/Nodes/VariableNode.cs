using System.Drawing;
using UnityEngine;

public class VariableNode : NodeBase
{
    [SerializeField] private VariableType _variableType;
    public VariableType VariableType
    {
        get
        {
            return _variableType;
        }
        set
        {
            _variableType = value;
        }
    }

    public VariableUIElement UIElement { get; set; }

    // Add this field to store the output value
    private ConnectorValueObject _outputValue;

    public override void Setup()
    {
        outputFields = new()
        {
            new VariableNodeField(_variableType).SetFunc(Output).ProvideDefaultValue(new ConnectorValueObject(null))
        };
    }

    [NodeValue("Value", typeof(object), KnownColor.Gray)]
    public void Output(ConnectorValueObject value)
    {
        _outputValue = value; // Store reference
        UpdateOutputValue(); // Set initial value
    }

    // Add this method to update the output value
    public void UpdateOutputValue()
    {
        if (_outputValue != null && UIElement != null)
        {
            var val = UIElement.GetValue();
            Debug.Log($"VariableNode: Setting output value to {val} (type: {val?.GetType()})");
            _outputValue.SetValue(val);
        }
    }
}