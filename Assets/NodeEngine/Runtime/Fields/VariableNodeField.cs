using System;
using System.Reflection;
using UnityEngine;

public class VariableNodeField : NodeFieldBase
{
    private IConnectorValue _currentValue;
    private Action<IConnectorValue> _valueHandler;
    private VariableType _variableType;

    public VariableNodeField(VariableType variableType)
    {
        _variableType = variableType;
        CreateDefaultValue();
    }

    private void CreateDefaultValue()
    {
        _currentValue = _variableType switch
        {
            VariableType.Int => new ConnectorValueInt(0),
            VariableType.Single => new ConnectorValueFloat(0f),
            VariableType.Bool => new ConnectorValueBool(false),
            VariableType.String => new ConnectorValueString(""),
            _ => new ConnectorValueObject(null)
        };
    }

    public VariableNodeField SetFunc(Action<IConnectorValue> handler)
    {
        _valueHandler = handler;
        return this;
    }

    // ✅ NEW: Data-only propagation for VariableNodeField
    public override void UpdateValueFromSource(IConnectorValue sourceValue)
    {
        if (sourceValue == null) return;

        try
        {
            var innerValue = sourceValue.GetInnerValue();
            SetValue(innerValue);

            // Invoke handler but don't propagate further
            _valueHandler?.Invoke(_currentValue);

            Debug.Log($"[VariableNodeField] Updated to: {innerValue}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update VariableNodeField value: {e.Message}");
        }
    }

    public override void ProceedValue()
    {
        // Propagate to connected fields
        if (Connector?.Connections?.Count > 0)
        {
            foreach (var connectedConnector in Connector.Connections)
            {
                var field = connectedConnector?.Field;
                if (field != null && !connectedConnector.Node.IsProcessing)
                {
                    // Use data-only propagation
                    field.UpdateValueFromSource(_currentValue);
                }
            }
        }

        _valueHandler?.Invoke(_currentValue);
    }

    public override Type GetValueType() => GetConnectorType(_variableType);
    public override NodeValueAttribute GetAttribute() => _valueHandler?.GetMethodInfo()?.GetCustomAttribute<NodeValueAttribute>();
    public override object GetObjectValue() => _currentValue?.GetInnerValue();

    // Method to update the value
    public void SetValue(object newValue)
    {
        if (_currentValue != null)
        {
            switch (_currentValue)
            {
                case ConnectorValueInt intConnector when newValue is int intVal:
                    intConnector.SetValue(intVal);
                    break;
                case ConnectorValueFloat floatConnector when newValue is float floatVal:
                    floatConnector.SetValue(floatVal);
                    break;
                case ConnectorValueBool boolConnector when newValue is bool boolVal:
                    boolConnector.SetValue(boolVal);
                    break;
                case ConnectorValueString stringConnector when newValue is string stringVal:
                    stringConnector.SetValue(stringVal);
                    break;
                case ConnectorValueObject objConnector:
                    objConnector.SetValue(newValue);
                    break;
            }
        }
    }

    private Type GetConnectorType(VariableType variableType)
    {
        return variableType switch
        {
            VariableType.Int => typeof(int),
            VariableType.Single => typeof(float),
            VariableType.Bool => typeof(bool),
            VariableType.String => typeof(string),
            _ => typeof(object)
        };
    }
}