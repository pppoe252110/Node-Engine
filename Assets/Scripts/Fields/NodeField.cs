using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class NodeField<T> : NodeFieldBase where T : IConnectorValue
{
    public delegate void ValueHandlerFunc(T value);
    public event ValueHandlerFunc CurrentValueHandler;
    private T currentValue;
    private readonly bool _isInput;

    private IConnectorValueBridge _fastBridge;
    private bool _hasFastBridge;

    public NodeField(bool isInput)
    {
        _isInput = isInput;
        InitializeFastBridge();
    }

    private void InitializeFastBridge()
    {
        // 🚀 OPTIMIZATION: Skip bridge for void (no data to bridge)
        if (typeof(T) == typeof(ConnectorValueVoid))
        {
            _hasFastBridge = false;
            _fastBridge = null;

            return;
        }
        if (currentValue != null)
        {
            _fastBridge = ConnectorBridgeFactory.CreateBridge(currentValue);
            _hasFastBridge = _fastBridge != null;
        }
    }

    public NodeField<T> SetFunc(ValueHandlerFunc value)
    {
        CurrentValueHandler = value;
        return this;
    }

    public NodeField<T> ProvideDefaultValue(T value)
    {
        currentValue = value;

        // ✅ RE-INITIALIZE bridge when value is provided
        InitializeFastBridge();

        return this;
    }

    // ✅ NEW: Data-only propagation (no execution)
    public override void UpdateValueFromSource(IConnectorValue sourceValue)
    {
        if (sourceValue == null) return;

        try
        {
            var innerValue = sourceValue.GetInnerValue();

            // Update current value without triggering execution
            if (currentValue is IConnectorValueBridge targetBridge)
            {
                targetBridge.SetValueFast(innerValue);
            }
            else
            {
                SetValueManually(currentValue, innerValue);
            }

            // ✅ Invoke handler to update the value, but don't propagate further
            CurrentValueHandler?.Invoke(currentValue);
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
            CurrentValueHandler?.Invoke(currentValue);
            return;
        }

        // Handle INPUT fields (receiving values)
        if (_isInput && Connector.Connections.Count > 0)
        {
            var connectedConnector = Connector.Connections[Connector.Connections.Count - 1];

            if (connectedConnector?.Node == null)
            {
                CurrentValueHandler?.Invoke(currentValue);
                return;
            }

            var connectedNode = connectedConnector.Node;
            bool isVoid = connectedConnector.ValueType == typeof(void);

            if (!isVoid)
            {
                // DATA FLOW: Process connected node and get value
                if (!connectedNode.IsProcessing)
                {
                    connectedNode.Process();
                }

                var connectedField = connectedConnector.Field;
                if (connectedField != null)
                {
                    var sourceValue = connectedField.GetObjectValue();

                    if (sourceValue is IConnectorValue sourceConnector)
                    {
                        var innerValue = sourceConnector.GetInnerValue();

                        if (currentValue is IConnectorValueBridge targetBridge)
                        {
                            targetBridge.SetValueFast(innerValue);
                        }
                        else
                        {
                            SetValueManually(currentValue, innerValue);
                        }

                        CurrentValueHandler?.Invoke(currentValue);
                    }
                }
            }
            else
            {
                // VOID INPUT FLOW: Execute connected node
                if (Connector.Node is ExecutableNode exe && !exe.IsProcessing)
                {
                    exe.Process();
                    exe.Execute();
                }
            }
        }

        if (!_isInput)
        {
            CurrentValueHandler?.Invoke(currentValue);
            if (Connector.Connections.Count > 0)
            {
                if (Connector.ValueType != typeof(void))
                {
                    // DATA OUTPUT: Use data-only propagation
                    foreach (var connectedConnector in Connector.Connections)
                    {
                        var field = connectedConnector?.Field;
                        if (field != null && !connectedConnector.Node.IsProcessing)
                        {
                            field.UpdateValueFromSource(currentValue as IConnectorValue);
                        }
                    }
                }
                else
                {
                    // 🚀 OPTIMIZATION: Void output - direct execution (no bridge)
                    foreach (var connectedConnector in Connector.Connections)
                    {
                        var field = connectedConnector?.Field;
                        if (field != null && !connectedConnector.Node.IsProcessing)
                        {
                            field.ProceedValue();  // Trigger execution directly
                        }
                    }
                }
            }
        }
    }

    // Helper method for manual value setting
    private void SetValueManually(IConnectorValue target, object value)
    {
        if (target is ConnectorValueInt intTarget && value is int intVal)
            intTarget.SetValue(intVal);
        else if (target is ConnectorValueFloat floatTarget && value is float floatVal)
            floatTarget.SetValue(floatVal);
        else if (target is ConnectorValueBool boolTarget && value is bool boolVal)
            boolTarget.SetValue(boolVal);
        else if (target is ConnectorValueString stringTarget && value is string stringVal)
            stringTarget.SetValue(stringVal);
        else if (target is ConnectorValueObject objTarget)
            objTarget.SetValue(value);
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
            var wrapped = _fastBridge.WrappedValue;
            return wrapped;
        }
        return currentValue;
    }
}