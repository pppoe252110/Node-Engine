using System;
using System.Collections.Generic;
using System.Linq;
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

    public override IConnectorValue GetCurrentValue()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class NodeField<T> : NodeFieldBase<T> where T : IConnectorValue
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
            if (sourceValue is T directConnector)
            {
                _currentValue = directConnector;
                CurrentValueHandler?.Invoke(_currentValue);
            }
            else if (sourceValue is IConnectorValue<T> typedSource)
            {
                UpdateValueFromSourceTyped(typedSource);
            }
            else
            {
                var innerValue = sourceValue.GetInnerValue();
                UpdateCurrentValue(innerValue);
                CurrentValueHandler?.Invoke(_currentValue);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update value from source: {e.Message}");
        }
    }
    private void UpdateValueFromSourceTyped(IConnectorValue<T> sourceValue)
    {
        try
        {
            UpdateCurrentValueTyped(sourceValue);
            CurrentValueHandler?.Invoke(_currentValue);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update value from source (typed): {e.Message}");
        }
    }

    private void UpdateCurrentValue(object newValue)
    {
        if (_currentValue == null)
        {
            Debug.LogWarning("Current value is null in UpdateCurrentValue");
            return;
        }

        try
        {
            // If _currentValue is a connector wrapper, update its inner value
            if (_currentValue is IConnectorValue currentConnector)
            {
                currentConnector.SetInnerValue(newValue);
            }
            else
            {
                // Direct assignment (shouldn't happen for connector types)
                _currentValue = (T)newValue;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update current value: {e.Message}");
        }
    }

    private void UpdateCurrentValueTyped(IConnectorValue<T> newValue)
    {
        var innerValue = newValue.GetInnerValue();

        // Direct typed updates to avoid boxing
        if (_currentValue is ConnectorValueInt intConnector && innerValue is int intVal)
        {
            intConnector.SetInnerValue(intVal);
        }
        else if (_currentValue is ConnectorValueFloat floatConnector && innerValue is float floatVal)
        {
            floatConnector.SetInnerValue(floatVal);
        }
        else if (_currentValue is ConnectorValueBool boolConnector && innerValue is bool boolVal)
        {
            boolConnector.SetInnerValue(boolVal);
        }
        else if (_currentValue is ConnectorValueString stringConnector && innerValue is string stringVal)
        {
            stringConnector.SetInnerValue(stringVal);
        }
        else if (_currentValue is ConnectorValueVector3 vectorConnector && innerValue is Vector3 vectorVal)
        {
            vectorConnector.SetInnerValue(vectorVal);
        }
        else if (_currentValue is ConnectorValueObject objectConnector)
        {
            objectConnector.SetInnerValue(innerValue);
        }
        else if (_currentValue is ConnectorValueVoid)
        {
            // Void, do nothing
        }
        else
        {
            Debug.LogError($"Unsupported type for typed update: {_currentValue.GetType()}");
        }
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

        var connectedField = connectedConnector.Field as NodeFieldBase;  // Cast to base class
        if (connectedField == null)
        {
            Debug.LogError($"Connected field is not a NodeFieldBase: {connectedConnector.Field.GetType()}");
            return;
        }

        connectedField.ProceedValue();
        var sourceValue = connectedField.GetCurrentValue();  // Direct access (no reflection)

        // Direct value assignment for connector types
        if (sourceValue is T directValue)
        {
            _currentValue = directValue;
            CurrentValueHandler?.Invoke(_currentValue);
        }
        // Get inner value and update our current connector (handles object inputs)
        else if (_currentValue is IConnectorValue currentConnector)
        {
            var innerValue = sourceValue.GetInnerValue();
            currentConnector.SetInnerValue(innerValue);
            CurrentValueHandler?.Invoke(_currentValue);
        }
        else
        {
            // Fallback
            UpdateValueFromSource(sourceValue);
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
                field.UpdateValueFromSource(_currentValue);
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
}