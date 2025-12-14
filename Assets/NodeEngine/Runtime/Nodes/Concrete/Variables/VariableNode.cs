using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public abstract class VariableNode : NodeBase, ISerializableVariable
{
    public abstract VariableType VariableType { get; }
    public VariableUIElement UIElement { get; set; }
    protected NodeFieldBase _outputField;

    private IConnectorValue _cachedValue;

    public override void Setup()
    {
        InitializeCachedValues();

        _outputField = CreateTypedOutputField();
        outputFields = new() { _outputField };
    }

    private void InitializeCachedValues()
    {
        _cachedValue = VariableType switch
        {
            VariableType.Int => new ConnectorValueInt(0),
            VariableType.Single => new ConnectorValueFloat(0f),
            VariableType.Bool => new ConnectorValueBool(false),
            VariableType.String => new ConnectorValueString(""),
            VariableType.Vector3 => new ConnectorValueVector3(Vector3.zero),
            VariableType.Type => new ConnectorValueType(typeof(object)),
            _ => new ConnectorValueObject(null)
        };
    }

    private NodeFieldBase CreateTypedOutputField()
    {
        return VariableType switch
        {
            VariableType.Int => new NodeFieldTyped<ConnectorValueInt>()
                .SetHandler(OutputInt)
                .SetDefaultValue((ConnectorValueInt)_cachedValue),

            VariableType.Single => new NodeFieldTyped<ConnectorValueFloat>()
                .SetHandler(OutputFloat)
                .SetDefaultValue((ConnectorValueFloat)_cachedValue),

            VariableType.Bool => new NodeFieldTyped<ConnectorValueBool>()
                .SetHandler(OutputBool)
                .SetDefaultValue((ConnectorValueBool)_cachedValue),

            VariableType.String => new NodeFieldTyped<ConnectorValueString>()
                .SetHandler(OutputString)
                .SetDefaultValue((ConnectorValueString)_cachedValue),

            VariableType.Vector3 => new NodeFieldTyped<ConnectorValueVector3>()
                .SetHandler(OutputVector3)
                .SetDefaultValue((ConnectorValueVector3)_cachedValue),

            VariableType.Type => new NodeFieldTyped<ConnectorValueType>()
                .SetHandler(OutputType)
                .SetDefaultValue((ConnectorValueType)_cachedValue),

            _ => new NodeFieldTyped<ConnectorValueObject>()
                .SetHandler(OutputObject)
                .SetDefaultValue((ConnectorValueObject)_cachedValue)
        };
    }

    [NodeValue("Value", typeof(int))]
    public void OutputInt(ConnectorValueInt value)
    {
        // Direct cast since we know the type
        var cached = (ConnectorValueInt)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    [NodeValue("Value", typeof(float))]
    public void OutputFloat(ConnectorValueFloat value)
    {
        var cached = (ConnectorValueFloat)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    [NodeValue("Value", typeof(bool))]
    public void OutputBool(ConnectorValueBool value)
    {
        var cached = (ConnectorValueBool)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    [NodeValue("Value", typeof(string))]
    public void OutputString(ConnectorValueString value)
    {
        var cached = (ConnectorValueString)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    [NodeValue("Value", typeof(Vector3))]
    public void OutputVector3(ConnectorValueVector3 value)
    {
        var cached = (ConnectorValueVector3)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    [NodeValue("Value", typeof(Type))]
    public void OutputType(ConnectorValueType value)
    {
        var cached = (ConnectorValueType)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    [NodeValue("Value", typeof(object))]
    public void OutputObject(ConnectorValueObject value)
    {
        var cached = (ConnectorValueObject)_cachedValue;
        value.SetInnerValue(cached.GetInnerValue());
    }

    public void UpdateOutputValue()
    {
        if (UIElement != null)
        {
            object uiValue = UIElement.GetValue();

            switch (VariableType)
            {
                case VariableType.Int:
                    if (uiValue is int intVal)
                        ((ConnectorValueInt)_cachedValue).SetInnerValue(intVal);
                    break;
                case VariableType.Single:
                    if (uiValue is float floatVal)
                        ((ConnectorValueFloat)_cachedValue).SetInnerValue(floatVal);
                    break;
                case VariableType.Bool:
                    if (uiValue is bool boolVal)
                        ((ConnectorValueBool)_cachedValue).SetInnerValue(boolVal);
                    break;
                case VariableType.String:
                    if (uiValue is string stringVal)
                        ((ConnectorValueString)_cachedValue).SetInnerValue(stringVal);
                    break;
                case VariableType.Vector3:
                    if (uiValue is Vector3 vectorVal)
                        ((ConnectorValueVector3)_cachedValue).SetInnerValue(vectorVal);
                    break;
                case VariableType.Type:
                    if (uiValue is Type typeVal)
                        ((ConnectorValueType)_cachedValue).SetInnerValue(typeVal);
                    break;
                default:
                    ((ConnectorValueObject)_cachedValue).SetInnerValue(uiValue);
                    break;
            }
        }

        _outputField?.ProceedValue();
    }

    // Property to get the cached value
    public IConnectorValue CachedValue => _cachedValue;

    // Method to get the inner value
    public object GetValue()
    {
        return _cachedValue?.GetInnerValue();
    }

    // In VariableNode.cs, update the SetValue method and add helper methods:

    // Update just the SetValue and related methods:

    public void SetValue(object value)
    {
        if (_cachedValue != null)
        {
            _cachedValue.SetInnerValue(value);

            // Update UI if it exists
            if (UIElement != null)
            {
                // Different UI elements need different update methods
                if (UIElement is InputFieldVariableUI inputFieldUI)
                {
                    inputFieldUI.SetValue(value);
                }
                else if (UIElement is DropdownUIElement dropdownUI)
                {
                    UpdateDropdownUI(dropdownUI, value);
                }
                else
                {
                    Debug.LogWarning($"Unknown UIElement type: {UIElement.GetType()}");
                }
            }
            else
            {
                Debug.LogWarning("UIElement is null!");
            }

            // Trigger output
            UpdateOutputValue();
        }
    }

    private void UpdateDropdownUI(DropdownUIElement dropdownUI, object value)
    {
        if (value is Type typeValue)
        {
            // Use SetSelectedType with updateNode = FALSE to prevent recursion
            dropdownUI.SetSelectedType(typeValue, false);
        }
    }

    // Also update the SyncUIWithCachedValue method:
    public void SyncUIWithCachedValue()
    {
        if (UIElement != null)
        {
            // Get the current cached value
            object currentValue = GetValue();

            // Update the UI element with the cached value
            if (UIElement is InputFieldVariableUI inputFieldUI)
            {
                inputFieldUI.SetValue(currentValue);
            }
            else if (UIElement is DropdownUIElement dropdownUI)
            {
                // Use SetSelectedType with updateNode = FALSE
                dropdownUI.SetSelectedType(currentValue as Type, false);
            }

            // Now trigger the output
            UpdateOutputValue();
        }
    }
    public override void Process(List<Connector> fromConnectors = null)
    {
        // Variable nodes output their value when processed
        if (IsProcessing)
            return;

        IsProcessing = true;

        try
        {
            // Output the current value
            _outputField?.ProceedValue();
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public void DeserializeValue(string serializedValue)
    {
        CachedValue.SetInnerValue(serializedValue);
    }

    public string SerializeValue()
    {
        return JsonConvert.SerializeObject(CachedValue.GetInnerValue());
    }
}