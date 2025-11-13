using System;
using System.Collections.Concurrent;
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

    private delegate void SetValueDelegate(object target, object value);
    private static readonly ConcurrentDictionary<Type, Action<object, object>> _setValueDelegateCache = new();

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
        if (Connector == null) return;

        if (_isInput && Connector.Connections.Count > 0)
        {
            var connectedConnector = Connector.Connections[Connector.Connections.Count - 1]; // Faster than LastOrDefault
            if (connectedConnector?.Node == null) return;

            var connectedNode = connectedConnector.Node;
            bool isVoid = connectedConnector.ValueType == typeof(void);

            if (!isVoid)
            {
                connectedNode.Process(); // Process FIRST

                var connectedField = connectedConnector.Field;
                if (connectedField?.GetObjectValue() is IConnectorValue connectedValue)
                {
                    var innerValue = connectedValue.GetInnerValue();
                    if (innerValue != null)
                    {
                        // Use the fast path
                        SetValueFast(innerValue);
                    }
                }
            }

            if (isVoid && Connector.Node is ExecutableNode exe)
            {
                exe.Process();
                exe.Execute();
            }
        }

        CurrentValueHandler?.Invoke(currentValue);

        if (!_isInput && Connector.Connections.Count > 0)
        {
            foreach (var connectedConnector in Connector.Connections)
            {
                var field = connectedConnector?.Field;
                if (field != null)
                {
                    if (connectedConnector.ValueType != typeof(void))
                    {
                        Connector.Node.Process();
                    }
                    field.ProceedValue();
                }
            }
        }
    }

    private void SetValueFast(object innerValue)
    {
        switch (currentValue)
        {
            case ConnectorValueInt intConnector when innerValue is int intVal:
                intConnector.SetValue(intVal);
                return;
            case ConnectorValueSingle floatConnector when innerValue is float floatVal:
                floatConnector.SetValue(floatVal);
                return;
            case ConnectorValueString stringConnector when innerValue is string stringVal:
                stringConnector.SetValue(stringVal);
                return;
            case ConnectorValueBool boolConnector when innerValue is bool boolVal:
                boolConnector.SetValue(boolVal);
                return;
            case ConnectorValueObject objConnector:
                objConnector.SetValue(innerValue);
                return;
            default:
                // Only use cached delegates for uncommon types
                SetValueUsingCache(currentValue, innerValue);
                break;
        }
    }

    private void SetValueUsingCache(object target, object value)
    {
        var targetType = target.GetType();

        if (!_setValueDelegateCache.TryGetValue(targetType, out var setter))
        {
            setter = CreateOptimizedSetter(targetType);
            _setValueDelegateCache[targetType] = setter;
        }

        setter?.Invoke(target, value);
    }

    private static Action<object, object> CreateOptimizedSetter(Type targetType)
    {
        // Try to find the most specific SetValue method
        var setValueMethod = targetType.GetMethod("SetValue", BindingFlags.Public | BindingFlags.Instance);
        if (setValueMethod == null) return null;

        var parameters = setValueMethod.GetParameters();
        if (parameters.Length != 1) return null;

        var paramType = parameters[0].ParameterType;

        // Create optimized delegates for common types
        if (paramType == typeof(object))
        {
            return (target, value) => setValueMethod.Invoke(target, new[] { value });
        }

        // For value types, create type-specific fast paths
        if (paramType == typeof(int))
        {
            var typedDelegate = (Action<IConnectorValue, int>)Delegate.CreateDelegate(
                typeof(Action<IConnectorValue, int>), setValueMethod);
            return (target, value) =>
            {
                if (value is int intVal)
                    typedDelegate((IConnectorValue)target, intVal);
            };
        }
        else if (paramType == typeof(float))
        {
            var typedDelegate = (Action<IConnectorValue, float>)Delegate.CreateDelegate(
                typeof(Action<IConnectorValue, float>), setValueMethod);
            return (target, value) =>
            {
                if (value is float floatVal)
                    typedDelegate((IConnectorValue)target, floatVal);
            };
        }
        else if (paramType == typeof(string))
        {
            var typedDelegate = (Action<IConnectorValue, string>)Delegate.CreateDelegate(
                typeof(Action<IConnectorValue, string>), setValueMethod);
            return (target, value) =>
            {
                if (value is string stringVal)
                    typedDelegate((IConnectorValue)target, stringVal);
            };
        }
        else if (paramType == typeof(bool))
        {
            var typedDelegate = (Action<IConnectorValue, bool>)Delegate.CreateDelegate(
                typeof(Action<IConnectorValue, bool>), setValueMethod);
            return (target, value) =>
            {
                if (value is bool boolVal)
                    typedDelegate((IConnectorValue)target, boolVal);
            };
        }

        return (target, value) =>
        {
            if (value != null && paramType.IsAssignableFrom(value.GetType()))
                setValueMethod.Invoke(target, new[] { value });
        };
    }
}