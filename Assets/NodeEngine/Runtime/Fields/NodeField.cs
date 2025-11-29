// Assets/NodeEngine/Runtime/Fields/NodeField.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// =================================================================================
// PART 1: The NON-GENERIC NodeField
// This is used for execution flow (void).
// =================================================================================
[Serializable]
public class NodeField : NodeFieldBase
{
    public delegate void ValueHandlerFunc(IConnectorValue value);
    public event ValueHandlerFunc CurrentValueHandler;

    public NodeField SetHandler(ValueHandlerFunc handler)
    {
        CurrentValueHandler = handler;
        return this;
    }

    public override Type GetValueType() => typeof(void);
    public override NodeValueAttribute GetAttribute() => CurrentValueHandler?.GetMethodInfo()?.GetCustomAttribute<NodeValueAttribute>();
    public override object GetObjectValue() => null; // Execution flow has no object value

public override void ProceedValue()
{
    ProceedValue(new HashSet<NodeBase>());
}

private void ProceedValue(HashSet<NodeBase> processedNodes)
{
    if (Connector?.Node == null) return;
    
    // Prevent infinite recursion
    if (processedNodes.Contains(Connector.Node))
        return;
        
    processedNodes.Add(Connector.Node);

    // 1. Trigger our own handler
    var value = Connector?.GetConnectorValue() ?? ConnectorValueVoid.Instance;
    CurrentValueHandler?.Invoke(value);

    // 2. Propagate the execution signal to all connected fields.
    if (Connector != null)
    {
        foreach (var connectedConnector in Connector.Connections)
        {
            var field = connectedConnector?.Field;
            if (field != null && !connectedConnector.Node.IsProcessing && connectedConnector.Node != Connector.Node)
            {
                // Pass the processedNodes set to track which nodes we've already visited
                if (field is NodeField executionField)
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
}


    // This method is not used for execution flow.
    public override void UpdateValueFromSource(IConnectorValue sourceValue) { }
}


// =================================================================================
// PART 2: The GENERIC NodeField<T>
// This is used for data types like int, float, bool, Vector3, etc.
// =================================================================================
[Serializable]
public class NodeField<T> : NodeFieldBase<T>
{
    public delegate void ValueHandlerFunc(T value);
    public event ValueHandlerFunc CurrentValueHandler;

    public NodeField(bool isInput)
    {
        // Initialize with a default value of the correct type
        if (typeof(T) == typeof(ConnectorValueInt))
            _currentValue = (T)(object)new ConnectorValueInt(0);
        else if (typeof(T) == typeof(ConnectorValueFloat))
            _currentValue = (T)(object)new ConnectorValueFloat(0f);
        else if (typeof(T) == typeof(ConnectorValueBool))
            _currentValue = (T)(object)new ConnectorValueBool(false);
        else if (typeof(T) == typeof(ConnectorValueString))
            _currentValue = (T)(object)new ConnectorValueString("");
        else if (typeof(T) == typeof(ConnectorValueVector3))
            _currentValue = (T)(object)new ConnectorValueVector3(Vector3.zero);
        else if (typeof(T) == typeof(ConnectorValueVoid))
            _currentValue = (T)(object)ConnectorValueVoid.Instance;
        else
            _currentValue = default;
    }

    public NodeField<T> SetHandler(ValueHandlerFunc handler)
    {
        CurrentValueHandler = handler;
        return this;
    }

    public NodeField<T> SetDefaultValue(T value)
    {
        _currentValue = value;
        return this;
    }

    public override void UpdateValueFromSource(IConnectorValue sourceValue)
    {
        if (sourceValue == null) return;

        try
        {
            object sourceInnerValue = sourceValue.GetInnerValue();
            UpdateCurrentValue(sourceInnerValue);
            CurrentValueHandler?.Invoke(_currentValue);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update value from source: {e.Message}");
        }
    }

    private void UpdateCurrentValue(object newValue)
    {
        if (typeof(T) == typeof(ConnectorValueInt))
            ((ConnectorValueInt)(object)_currentValue).SetInnerValue((int)newValue);
        else if (typeof(T) == typeof(ConnectorValueFloat))
            ((ConnectorValueFloat)(object)_currentValue).SetInnerValue((float)newValue);
        else if (typeof(T) == typeof(ConnectorValueBool))
            ((ConnectorValueBool)(object)_currentValue).SetInnerValue((bool)newValue);
        else if (typeof(T) == typeof(ConnectorValueString))
            ((ConnectorValueString)(object)_currentValue).SetInnerValue((string)newValue);
        else if (typeof(T) == typeof(ConnectorValueVector3))
            ((ConnectorValueVector3)(object)_currentValue).SetInnerValue((Vector3)newValue);
        else if (typeof(T) == typeof(ConnectorValueObject))
            ((ConnectorValueObject)(object)_currentValue).SetInnerValue(newValue);
    }

    public override void ProceedValue()
    {
        if (Connector == null)
        {
            CurrentValueHandler?.Invoke(_currentValue);
            return;
        }

        bool isInput = Connector.Node.inputConnectors.Contains(Connector);

        if (isInput)
        {
            ProcessInputField();
        }
        else
        {
            ProcessOutputField();
        }
    }

    private void ProcessInputField()
    {
        if (Connector.Connections.Count == 0)
        {
            CurrentValueHandler?.Invoke(_currentValue);
            return;
        }

        var connectedConnector = Connector.Connections[Connector.Connections.Count - 1];

        if (connectedConnector?.Node == null)
        {
            CurrentValueHandler?.Invoke(_currentValue);
            return;
        }

        bool isVoid = connectedConnector.ValueType == typeof(void);

        if (!isVoid)
        {
            ProcessDataInput(connectedConnector);
        }
        else
        {
            ProcessVoidInput(connectedConnector);
        }
    }

    private void ProcessDataInput(Connector connectedConnector)
    {
        // Automatic: Call Process() on the connected node to trigger its output computation
        if (!connectedConnector.Node.IsProcessing)
        {
            connectedConnector.Node.Process();
        }

        var connectedField = connectedConnector.Field;
        if (connectedField != null)
        {
            var sourceValue = connectedField.GetObjectValue();
            UpdateFromSourceValue(sourceValue);
        }
    }

    private void ProcessVoidInput(Connector connectedConnector)
    {
        // Simplified: Directly call Execute() on executable nodes (matches old behavior)
        if (Connector.Node is ExecutableNodeBase executableNode && !executableNode.IsProcessing)
        {
            executableNode.Execute();
        }
    }


    private void ProcessOutputField()
    {
        CurrentValueHandler?.Invoke(_currentValue);

        if (Connector.Connections.Count > 0)
        {
            if (Connector.ValueType != typeof(void))
            {
                ProcessDataOutput();
            }
            else
            {
                ProcessVoidOutput();
            }
        }
    }

    private void ProcessDataOutput()
    {
        // Propagate data to connected input fields
        foreach (var connectedConnector in Connector.Connections)
        {
            var field = connectedConnector?.Field;
            if (field != null && !connectedConnector.Node.IsProcessing)
            {
                field.UpdateValueFromSource(_currentValue as IConnectorValue);
            }
        }
    }

    private void ProcessVoidOutput()
    {
        // Propagate execution to connected input fields
        foreach (var connectedConnector in Connector.Connections)
        {
            var field = connectedConnector?.Field;
            if (field != null && !connectedConnector.Node.IsProcessing)
            {
                field.ProceedValue();
            }
        }
    }

    private void UpdateFromSourceValue(object sourceValue)
    {
        if (sourceValue is IConnectorValue sourceConnector)
        {
            var innerValue = sourceConnector.GetInnerValue();
            UpdateCurrentValue(innerValue);
            CurrentValueHandler?.Invoke(_currentValue);
        }
    }

    public override NodeValueAttribute GetAttribute()
    {
        return CurrentValueHandler?.GetMethodInfo()?.GetCustomAttribute<NodeValueAttribute>();
    }

    public override T GetValue()
    {
        if (Connector?.Connections.Count > 0 && Connector.Node.inputConnectors.Contains(Connector))
        {
            var sourceConnector = Connector.Connections[0];
            if (sourceConnector?.GetConnectorValue() is IConnectorValue<T> sourceValue)
            {
                return sourceValue.GetInnerValue();
            }
        }

        return _currentValue;
    }

    public override object GetObjectValue()
    {
        return GetValue();
    }
}