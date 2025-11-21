// Fields/FastConnectorBridge.cs
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class FastConnectorBridge<T> : ITypedConnectorBridge<T>
{
    private readonly IConnectorValue _wrappedValue;
    private static Action<IConnectorValue, T> _staticSetter;
    private static Func<IConnectorValue, T> _staticGetter;
    private static bool _isCompiled;
    private Action<IConnectorValue, T> _setter;
    private Func<IConnectorValue, T> _getter;

    public IConnectorValue WrappedValue => _wrappedValue;
    public Type ValueType => typeof(T);

    public FastConnectorBridge(IConnectorValue wrappedValue)
    {
        _wrappedValue = wrappedValue ?? throw new ArgumentNullException(nameof(wrappedValue));

        // 🚀 OPTIMIZATION: Compile once per type
        if (!_isCompiled)
        {
            _staticSetter = CreateSetter(wrappedValue.GetType());
            _staticGetter = CreateGetter(wrappedValue.GetType());
            _isCompiled = true;
        }

        _setter = _staticSetter;
        _getter = _staticGetter;

        if (_setter == null || _getter == null)
        {
            throw new InvalidOperationException($"Could not create fast accessors for type {wrappedValue.GetType()}");
        }
    }

    private static Action<IConnectorValue, T> CreateSetter(Type wrappedType)
    {
        if (!typeof(IConnectorValue).IsAssignableFrom(wrappedType))
            throw new ArgumentException($"Type {wrappedType} does not implement IConnectorValue");

        // Look for SetValue method first
        var setValueMethod = wrappedType.GetMethod("SetValue", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(T) }, null);
        if (setValueMethod != null)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var valueParam = Expression.Parameter(typeof(T));
            var castConnector = Expression.Convert(connectorParam, wrappedType);
            var call = Expression.Call(castConnector, setValueMethod, valueParam);
            return Expression.Lambda<Action<IConnectorValue, T>>(call, connectorParam, valueParam).Compile();
        }

        // Look for field
        var valueField = wrappedType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance)
                      ?? wrappedType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance)
                      ?? wrappedType.GetField("m_value", BindingFlags.NonPublic | BindingFlags.Instance);

        if (valueField != null && valueField.FieldType == typeof(T))
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var valueParam = Expression.Parameter(typeof(T));
            var castConnector = Expression.Convert(connectorParam, wrappedType);
            var fieldAccess = Expression.Field(castConnector, valueField);
            var assign = Expression.Assign(fieldAccess, valueParam);
            return Expression.Lambda<Action<IConnectorValue, T>>(assign, connectorParam, valueParam).Compile();
        }

        // Look for property
        var valueProperty = wrappedType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
                         ?? wrappedType.GetProperty("value", BindingFlags.Public | BindingFlags.Instance);

        if (valueProperty != null && valueProperty.PropertyType == typeof(T) && valueProperty.CanWrite)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var valueParam = Expression.Parameter(typeof(T));
            var castConnector = Expression.Convert(connectorParam, wrappedType);
            var propertyAccess = Expression.Property(castConnector, valueProperty);
            var assign = Expression.Assign(propertyAccess, valueParam);
            return Expression.Lambda<Action<IConnectorValue, T>>(assign, connectorParam, valueParam).Compile();
        }

        return null;
    }

    private static Func<IConnectorValue, T> CreateGetter(Type wrappedType)
    {
        if (!typeof(IConnectorValue).IsAssignableFrom(wrappedType))
            throw new ArgumentException($"Type {wrappedType} does not implement IConnectorValue");

        // Look for GetValue method first
        var getValueMethod = wrappedType.GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance);
        if (getValueMethod != null && getValueMethod.ReturnType == typeof(T) && getValueMethod.GetParameters().Length == 0)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var castConnector = Expression.Convert(connectorParam, wrappedType);
            var call = Expression.Call(castConnector, getValueMethod);
            return Expression.Lambda<Func<IConnectorValue, T>>(call, connectorParam).Compile();
        }

        // Look for field
        var valueField = wrappedType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance)
                      ?? wrappedType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance)
                      ?? wrappedType.GetField("m_value", BindingFlags.NonPublic | BindingFlags.Instance);

        if (valueField != null && valueField.FieldType == typeof(T))
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var castConnector = Expression.Convert(connectorParam, wrappedType);
            var fieldAccess = Expression.Field(castConnector, valueField);
            return Expression.Lambda<Func<IConnectorValue, T>>(fieldAccess, connectorParam).Compile();
        }

        // Look for property
        var valueProperty = wrappedType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
                         ?? wrappedType.GetProperty("value", BindingFlags.Public | BindingFlags.Instance);

        if (valueProperty != null && valueProperty.PropertyType == typeof(T) && valueProperty.CanRead)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var castConnector = Expression.Convert(connectorParam, wrappedType);
            var propertyAccess = Expression.Property(castConnector, valueProperty);
            return Expression.Lambda<Func<IConnectorValue, T>>(propertyAccess, connectorParam).Compile();
        }

        return null;
    }

    public void SetValue(T value) => _setter(_wrappedValue, value);
    public T GetValue() => _getter(_wrappedValue);
    public void SetValueFast(object value) => SetValue((T)value);
    public object GetValueFast() => GetValue();
}