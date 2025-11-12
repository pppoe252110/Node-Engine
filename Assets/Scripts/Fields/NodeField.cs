using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class NodeField<T> : NodeFieldBase where T : IConnectorValue
{
    public delegate void ValueHandlerFunc(T value);
    public event ValueHandlerFunc CurrentValueHandler;
    // Removed [SerializeField] to prevent serialization issues with cloning
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
        if (_isInput && Connector != null && Connector.Connections.Count > 0)
        {
            var connectedConnector = Connector.Connections[0];
            var connectedNode = connectedConnector.Node;

            // Process the entire connected node first to ensure its values are set
            connectedNode.Process();

            var connectedValue = connectedConnector.Field.GetObjectValue() as IConnectorValue;
            if (connectedValue != null && typeof(T) != typeof(ConnectorVoid))
            {
                var innerValue = connectedValue.GetInnerValue();
                if (currentValue is ConnectorValueBase<object> objBase)
                {
                    objBase.SetValue(innerValue);
                }
                else
                {
                    // For matching types, set directly via reflection
                    var setMethod = currentValue.GetType().GetMethod("SetValue", new Type[] { innerValue.GetType() });
                    if (setMethod != null)
                    {
                        setMethod.Invoke(currentValue, new object[] { innerValue });
                    }
                }
            }
        }
        CurrentValueHandler?.Invoke(currentValue);
    }
}