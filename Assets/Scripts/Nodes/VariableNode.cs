using System.Drawing;
using UnityEngine;

public class VariableNode : NodeBase
{
    [SerializeField] private VariableType _variableType;
    public VariableType VariableType
    {
        get => _variableType;
        set
        {
            _variableType = value;
            // Recreate output when type changes
            if (outputFields != null && outputFields.Count > 0)
            {
                Setup();
            }
        }
    }

    public VariableUIElement UIElement { get; set; }

    private NodeFieldBase _outputField;

    public override void Setup()
    {
        _outputField = CreateTypedOutputField();
        outputFields = new() { _outputField };
    }

    private NodeFieldBase CreateTypedOutputField()
    {
        switch (_variableType)
        {
            case VariableType.Int:
                return new NodeField<ConnectorValueInt>(isInput: false)
                    .SetFunc(OutputInt)
                    .ProvideDefaultValue(new ConnectorValueInt(0));

            case VariableType.Float:
                return new NodeField<ConnectorValueFloat>(isInput: false)
                    .SetFunc(OutputFloat)
                    .ProvideDefaultValue(new ConnectorValueFloat(0f));

            case VariableType.Bool:
                return new NodeField<ConnectorValueBool>(isInput: false)
                    .SetFunc(OutputBool)
                    .ProvideDefaultValue(new ConnectorValueBool(false));

            case VariableType.String:
                return new NodeField<ConnectorValueString>(isInput: false)
                    .SetFunc(OutputString)
                    .ProvideDefaultValue(new ConnectorValueString(""));

            default:
                return new NodeField<ConnectorValueObject>(isInput: false)
                    .SetFunc(OutputObject)
                    .ProvideDefaultValue(new ConnectorValueObject(null));
        }
    }

    // Separate methods for each type with correct attributes
    [NodeValue("Value", typeof(int), KnownColor.Red)]
    public void OutputInt(ConnectorValueInt value)
    {
        if (UIElement != null && UIElement.GetValue() is int uiVal)
        {
            value.SetValue(uiVal);
        }
    }
    [NodeValue("Value", typeof(float), KnownColor.Green)]
    public void OutputFloat(ConnectorValueFloat value) { }

    [NodeValue("Value", typeof(bool), KnownColor.Blue)]
    public void OutputBool(ConnectorValueBool value) { }

    [NodeValue("Value", typeof(string), KnownColor.Yellow)]
    public void OutputString(ConnectorValueString value) { }

    [NodeValue("Value", typeof(object), KnownColor.Gray)]
    public void OutputObject(ConnectorValueObject value) { }

    public void UpdateOutputValue()
    {
        if (UIElement != null && _outputField != null)
        {
            var val = UIElement.GetValue();

            // Update the value based on the current field type
            switch (_outputField)
            {
                case NodeField<ConnectorValueInt> intField:
                    // Get the actual ConnectorValueInt instance from the field
                    var intValue = intField.GetObjectValue() as ConnectorValueInt;
                    
                    if (intValue != null && val is int intVal)
                    {
                        intValue.SetValue(intVal);

                        // Force propagation to connected inputs
                        intField.ProceedValue();
                    }
                    else
                    {
                        Debug.LogError($"VariableNode: Failed to set value - intValue: {intValue != null}, val: {val}, val is int: {val is int}");
                    }
                    break;

                case NodeField<ConnectorValueFloat> floatField:
                    var floatValue = floatField.GetObjectValue() as ConnectorValueFloat;
                    if (floatValue != null && val is float floatVal)
                    {
                        floatValue.SetValue(floatVal);
                        floatField.ProceedValue();
                    }
                    break;

                case NodeField<ConnectorValueBool> boolField:
                    var boolValue = boolField.GetObjectValue() as ConnectorValueBool;
                    if (boolValue != null && val is bool boolVal)
                    {
                        boolValue.SetValue(boolVal);
                        boolField.ProceedValue();
                    }
                    break;

                case NodeField<ConnectorValueString> stringField:
                    var stringValue = stringField.GetObjectValue() as ConnectorValueString;
                    if (stringValue != null && val is string stringVal)
                    {
                        stringValue.SetValue(stringVal);
                        stringField.ProceedValue();
                    }
                    break;

                case NodeField<ConnectorValueObject> objField:
                    var objValue = objField.GetObjectValue() as ConnectorValueObject;
                    if (objValue != null)
                    {
                        objValue.SetValue(val);
                        objField.ProceedValue();
                    }
                    break;
            }
        }
    }
}