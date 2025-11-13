using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class NodeField<T> : NodeFieldBase where T : IConnectorValue
{
    public delegate void ValueHandlerFunc(T value);
    public event ValueHandlerFunc CurrentValueHandler;
    private T currentValue;
    private bool _isInput;

    public NodeField(bool isInput)
    {
        _isInput = isInput;
    }

    public NodeField<T> SetFunc(ValueHandlerFunc value)
    {
        CurrentValueHandler = value;
        return this;
    }

    public NodeField<T> ProvideDefaultValue(T value)
    {
        currentValue = value;
        return this;
    }

    public override NodeValueAttribute GetAttribute()
    {
        return CurrentValueHandler.GetMethodInfo().GetCustomAttribute<NodeValueAttribute>();
    }

    public override Type GetValueType()
    {
        return typeof(T);
    }

    public override object GetObjectValue()
    {
        return currentValue;
    }

    public override void ProceedValue()
    {
        if (Connector == null)
        {
            Debug.LogError("Connector is null in NodeField.ProceedValue()");
            return;
        }

        // For inputs: Update value from connected outputs
        if (_isInput && Connector.Connections.Count > 0)
        {
            var connectedConnector = Connector.Connections.LastOrDefault();
            if (connectedConnector == null || connectedConnector.Node == null)
            {
                Debug.LogWarning("Connected connector or node is null");
                return;
            }

            var connectedNode = connectedConnector.Node;
            bool isVoid = connectedConnector.ValueType == typeof(void);

            if (!isVoid)
            {
                connectedNode.Process(); // Process FIRST
                var connectedValue = connectedConnector.Field?.GetObjectValue() as IConnectorValue;
                if (connectedValue != null)
                {
                    var innerValue = connectedValue.GetInnerValue();
                    if (currentValue is ConnectorValueBase<object> objBase && innerValue != null)
                    {
                        objBase.SetValue(innerValue);
                    }
                    else
                    {
                        var setMethod = currentValue?.GetType().GetMethod("SetValue", new Type[] { innerValue?.GetType() ?? typeof(object) });
                        if (setMethod != null)
                        {
                            setMethod.Invoke(currentValue, new object[] { innerValue });
                        }
                        else
                        {
                            Debug.LogError($"No SetValue method for type {innerValue?.GetType()}");
                        }
                    }
                }
            }

            if (isVoid && Connector?.Node is ExecutableNode exe)
            {
                exe.Process();
                exe.Execute();
            }
        }

        CurrentValueHandler?.Invoke(currentValue);

        // For outputs: Trigger connected inputs
        if (!_isInput && Connector.Connections.Count > 0)
        {
            foreach (var connectedConnector in Connector.Connections)
            {
                if (connectedConnector?.Field != null)
                {
                    if (connectedConnector.ValueType != typeof(void))
                    {
                        Connector.Node.Process();
                    }
                    connectedConnector.Field.ProceedValue();
                }
            }
        }
    }
}