using System.Collections.Generic;
using UnityEngine;

public abstract class VariableNode : NodeBase
{
    public abstract VariableType VariableType { get; }
    public VariableUIElement UIElement { get; set; }
    protected NodeFieldBase _outputField;

    // Cached connector values
    private ConnectorValueInt _cachedIntValue;
    private ConnectorValueFloat _cachedFloatValue;
    private ConnectorValueBool _cachedBoolValue;
    private ConnectorValueString _cachedStringValue;
    private ConnectorValueVector3 _cachedVector3Value;
    private ConnectorValueObject _cachedObjectValue;

    public override void Setup()
    {
        // Initialize cached values with defaults
        InitializeCachedValues();

        _outputField = CreateTypedOutputField();
        outputFields = new() { _outputField };
    }

    private void InitializeCachedValues()
    {
        switch (VariableType)
        {
            case VariableType.Int:
                _cachedIntValue = new ConnectorValueInt(0);
                break;
            case VariableType.Single:
                _cachedFloatValue = new ConnectorValueFloat(0f);
                break;
            case VariableType.Bool:
                _cachedBoolValue = new ConnectorValueBool(false);
                break;
            case VariableType.String:
                _cachedStringValue = new ConnectorValueString("");
                break;
            case VariableType.Vector3:
                _cachedVector3Value = new ConnectorValueVector3(Vector3.zero);
                break;
            default:
                _cachedObjectValue = new ConnectorValueObject(null);
                break;
        }
    }

    private NodeFieldBase CreateTypedOutputField()
    {
        switch (VariableType)
        {
            case VariableType.Int:
                return new NodeField<ConnectorValueInt>(false)
                    .SetHandler(OutputInt)
                    .SetDefaultValue(_cachedIntValue);

            case VariableType.Single:
                return new NodeField<ConnectorValueFloat>(false)
                    .SetHandler(OutputFloat)
                    .SetDefaultValue(_cachedFloatValue);

            case VariableType.Bool:
                return new NodeField<ConnectorValueBool>(false)
                    .SetHandler(OutputBool)
                    .SetDefaultValue(_cachedBoolValue);

            case VariableType.String:
                return new NodeField<ConnectorValueString>(false)
                    .SetHandler(OutputString)
                    .SetDefaultValue(_cachedStringValue);

            case VariableType.Vector3:
                return new NodeField<ConnectorValueVector3>(false)
                    .SetHandler(OutputVector3)
                    .SetDefaultValue(_cachedVector3Value);

            default:
                return new NodeField<ConnectorValueObject>(false)
                    .SetHandler(OutputObject)
                    .SetDefaultValue(_cachedObjectValue);
        }
    }

    [NodeValue("Value", typeof(int))]
    public void OutputInt(ConnectorValueInt value)
    {
        // Just pass the cached value without updating it
        value.SetInnerValue(_cachedIntValue.GetInnerValue());
    }

    [NodeValue("Value", typeof(float))]
    public void OutputFloat(ConnectorValueFloat value)
    {
        value.SetInnerValue(_cachedFloatValue.GetInnerValue());
    }

    [NodeValue("Value", typeof(bool))]
    public void OutputBool(ConnectorValueBool value)
    {
        value.SetInnerValue(_cachedBoolValue.GetInnerValue());
    }

    [NodeValue("Value", typeof(string))]
    public void OutputString(ConnectorValueString value)
    {
        value.SetInnerValue(_cachedStringValue.GetInnerValue());
    }

    [NodeValue("Value", typeof(Vector3))]
    public void OutputVector3(ConnectorValueVector3 value)
    {
        value.SetInnerValue(_cachedVector3Value.GetInnerValue());
    }

    [NodeValue("Value", typeof(object))]
    public void OutputObject(ConnectorValueObject value)
    {
        value.SetInnerValue(_cachedObjectValue.GetInnerValue());
    }

    public void UpdateOutputValue()
    {
        // Update the cached value from UI element first
        if (UIElement != null)
        {
            object uiValue = UIElement.GetValue();

            switch (VariableType)
            {
                case VariableType.Int:
                    if (uiValue is int intVal)
                        _cachedIntValue.SetInnerValue(intVal);
                    break;
                case VariableType.Single:
                    if (uiValue is float floatVal)
                        _cachedFloatValue.SetInnerValue(floatVal);
                    break;
                case VariableType.Bool:
                    if (uiValue is bool boolVal)
                        _cachedBoolValue.SetInnerValue(boolVal);
                    break;
                case VariableType.String:
                    if (uiValue is string stringVal)
                        _cachedStringValue.SetInnerValue(stringVal);
                    break;
                case VariableType.Vector3:
                    if (uiValue is Vector3 vectorVal)
                        _cachedVector3Value.SetInnerValue(vectorVal);
                    break;
                default:
                    _cachedObjectValue.SetInnerValue(uiValue);
                    break;
            }
        }

        // Then proceed with the updated cached value
        _outputField?.ProceedValue();
    }
}