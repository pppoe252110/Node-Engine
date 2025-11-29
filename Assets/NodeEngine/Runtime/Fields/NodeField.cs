using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
    public override object GetObjectValue() => null; 

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

    var value = Connector?.GetConnectorValue() ?? ConnectorValueVoid.Instance;
    CurrentValueHandler?.Invoke(value);

    if (Connector != null)
    {
        foreach (var connectedConnector in Connector.Connections)
        {
            var field = connectedConnector?.Field;
            if (field != null && !connectedConnector.Node.IsProcessing && connectedConnector.Node != Connector.Node)
            {
                
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

    public override void UpdateValueFromSource(IConnectorValue sourceValue) { }
}

[Serializable]
public class NodeField<T> : NodeFieldBase<T>
{
    public delegate void ValueHandlerFunc(T value);
    public event ValueHandlerFunc CurrentValueHandler;

    public NodeField()
    {
        
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
        
        if (Connector.Node is ExecutableNodeBase executableNode && !executableNode.IsProcessing)
        {
            executableNode.Process();
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
