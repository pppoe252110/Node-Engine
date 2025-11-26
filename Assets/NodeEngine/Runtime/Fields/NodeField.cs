using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class NodeField<T> : NodeFieldBase where T : IConnectorValue
{
    public delegate void ValueHandlerFunc(T value);

    
    private T _currentValue;
    private readonly bool _isInput;
    private IConnectorValueBridge _fastBridge;
    private bool _hasFastBridge;

    
    public event ValueHandlerFunc CurrentValueHandler;
    public T CurrentValue => _currentValue;
    public bool HasFastBridge => _hasFastBridge;
    public bool IsInput => _isInput;

    public NodeField(bool isInput)
    {
        _isInput = isInput;
        InitializeFastBridge();
    }

    private void InitializeFastBridge()
    {
        
        if (typeof(T) == typeof(ConnectorValueVoid))
        {
            _hasFastBridge = false;
            _fastBridge = null;
            return;
        }

        if (_currentValue != null)
        {
            _fastBridge = ConnectorBridgeFactory.CreateBridge(_currentValue);
            _hasFastBridge = _fastBridge != null;
        }
    }

    
    public NodeField<T> SetHandler(ValueHandlerFunc handler)
    {
        CurrentValueHandler = handler;
        return this;
    }

    public NodeField<T> SetDefaultValue(T value)
    {
        _currentValue = value;
        InitializeFastBridge(); 
        return this;
    }

    
    public override void UpdateValueFromSource(IConnectorValue sourceValue)
    {
        if (sourceValue == null) return;

        try
        {
            var innerValue = sourceValue.GetInnerValue();
            UpdateCurrentValue(innerValue);

            
            CurrentValueHandler?.Invoke(_currentValue);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update value from source: {e.Message}");
        }
    }

    public override void ProceedValue()
    {
        if (Connector == null)
        {
            CurrentValueHandler?.Invoke(_currentValue);
            return;
        }

        if (_isInput)
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

    private void UpdateCurrentValue(object newValue)
    {
        if (_currentValue is IConnectorValueBridge targetBridge)
        {
            targetBridge.SetValueFast(newValue);
        }
        else
        {
            SetValueManually(_currentValue, newValue);
        }
    }

    
    private void SetValueManually(IConnectorValue target, object value)
    {
        switch (target)
        {
            case ConnectorValueInt intTarget when value is int intVal:
                intTarget.SetValue(intVal);
                break;
            case ConnectorValueFloat floatTarget when value is float floatVal:
                floatTarget.SetValue(floatVal);
                break;
            case ConnectorValueBool boolTarget when value is bool boolVal:
                boolTarget.SetValue(boolVal);
                break;
            case ConnectorValueString stringTarget when value is string stringVal:
                stringTarget.SetValue(stringVal);
                break;
            case ConnectorValueObject objTarget:
                objTarget.SetValue(value);
                break;
        }
    }

    public override NodeValueAttribute GetAttribute()
    {
        return CurrentValueHandler?.GetMethodInfo()?.GetCustomAttribute<NodeValueAttribute>();
    }

    public override Type GetValueType()
    {
        return typeof(T);
    }

    public override object GetObjectValue()
    {
        if (_hasFastBridge && _fastBridge != null)
        {
            return _fastBridge.WrappedValue;
        }
        return _currentValue;
    }
}