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
        // For inputs: Update value from connected outputs
        if (_isInput && Connector != null && Connector.Connections.Count > 0)
        {
            var connectedConnector = Connector.Connections.LastOrDefault();
            var connectedNode = connectedConnector.Node;

            // CORRECT void detection - check the actual type, not the value
            bool isVoid = connectedConnector.ValueType == typeof(void);

            // Process upstream for value updates (only if data input)
            if (!isVoid)
            {
                connectedNode.Process(); // Process FIRST to get updated values

                // Now get the updated value
                var connectedValue = connectedConnector.Field.GetObjectValue() as IConnectorValue;
                var innerValue = connectedValue?.GetInnerValue();

                if (connectedValue != null && innerValue != null)
                {
                    if (currentValue is ConnectorValueBase<object> objBase)
                    {
                        objBase.SetValue(innerValue);
                    }
                    else
                    {
                        var setMethod = currentValue.GetType().GetMethod("SetValue", new Type[] { innerValue.GetType() });
                        if (setMethod != null)
                        {
                            setMethod.Invoke(currentValue, new object[] { innerValue });
                        }
                        else
                        {
                            Debug.LogError($"No SetValue method found for type {innerValue.GetType()}");
                        }
                    }
                }
            }

            // For void inputs (events): Trigger Execute and propagate
            if (isVoid && Connector?.Node is ExecutableNode exe)
            {
                // Update data inputs before executing
                exe.Process();  // This will update data inputs like LogString
                exe.Execute();

                // Propagate to connected outputs (fire downstream events)
                //foreach (var outputConnector in exe.outputConnectors)
                //{
                //    if (outputConnector.ValueType == typeof(void))
                //    {
                //        outputConnector.Field.ProceedValue();
                //    }
                //}
            }
        }

        // Invoke handler (for both inputs and outputs)
        CurrentValueHandler?.Invoke(currentValue);

        // For outputs: Trigger connected inputs (BOTH data and void outputs need propagation)
        if (!_isInput && Connector != null && Connector.Connections.Count > 0)
        {
            foreach (var connectedConnector in Connector.Connections)
            {
                // For data outputs: ensure connected inputs get our value
                if (connectedConnector.ValueType != typeof(void))
                {
                    // Make sure our value is up to date first
                    Connector.Node.Process();
                }

                // Trigger the connected input
                connectedConnector.Field.ProceedValue();
            }
        }
    }
}