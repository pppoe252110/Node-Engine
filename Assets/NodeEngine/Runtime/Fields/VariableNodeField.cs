using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class VariableNodeField : NodeFieldBase
{
    private IConnectorValue _currentValue;
    private Action<IConnectorValue> _valueHandler;
    private VariableType _variableType;

    public override IConnectorValue GetCurrentValue() => _currentValue;
    
    public override void UpdateValueFromSource(IConnectorValue sourceValue)
    {
        if (sourceValue == null) return;

        try
        {
            var innerValue = sourceValue.GetInnerValue();
            SetValue(innerValue);
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
        ProceedValue(new HashSet<NodeBase>());
    }

    private void ProceedValue(HashSet<NodeBase> processedNodes)
    {
        if (Connector?.Node == null) return;

        if (processedNodes.Contains(Connector.Node))
            return;

        processedNodes.Add(Connector.Node);

        if (Connector?.Connections?.Count > 0)
        {
            foreach (var connectedConnector in Connector.Connections)
            {
                var field = connectedConnector?.Field;
                if (field != null && !connectedConnector.Node.IsProcessing && connectedConnector.Node != Connector.Node)
                {
                    // Check for VariableNodeField specifically to pass processedNodes
                    if (field is VariableNodeField executionField)
                    {
                        executionField.ProceedValue(processedNodes);
                    }
                    else
                    {
                        field.ProceedValue();
                    }
                }
            }
        }

        _valueHandler?.Invoke(_currentValue);
    }

    public override Type GetValueType() => GetConnectorType(_variableType);
    public override NodeValueAttribute GetAttribute() => _valueHandler?.GetMethodInfo()?.GetCustomAttribute<NodeValueAttribute>();

    public void SetValue(object newValue)
    {
        if (_currentValue != null)
        {
            switch (_currentValue)
            {
                case ConnectorValueInt intConnector when newValue is int intVal:
                    intConnector.SetInnerValue(intVal);
                    break;
                case ConnectorValueFloat floatConnector when newValue is float floatVal:
                    floatConnector.SetInnerValue(floatVal);
                    break;
                case ConnectorValueBool boolConnector when newValue is bool boolVal:
                    boolConnector.SetInnerValue(boolVal);
                    break;
                case ConnectorValueString stringConnector when newValue is string stringVal:
                    stringConnector.SetInnerValue(stringVal);
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
