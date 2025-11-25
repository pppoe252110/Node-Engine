using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FastConnectorBridge<T> : ITypedConnectorBridge<T>
{
    private readonly IConnectorValue _wrappedValue;

    
    private static Action<IConnectorValue, T> _staticSetter;
    private static Func<IConnectorValue, T> _staticGetter;
    private static bool _isCompiled;
    private static readonly object _compileLock = new object();

    public IConnectorValue WrappedValue => _wrappedValue;
    public Type ValueType => typeof(T);

    
    public FastConnectorBridge(IConnectorValue wrappedValue)
    {
        _wrappedValue = wrappedValue ?? throw new ArgumentNullException(nameof(wrappedValue));
        EnsureCompiled();
    }

    
    public FastConnectorBridge(IConnectorValue wrappedValue, Action<IConnectorValue, T> setter, Func<IConnectorValue, T> getter)
    {
        _wrappedValue = wrappedValue ?? throw new ArgumentNullException(nameof(wrappedValue));
        _staticSetter = setter;
        _staticGetter = getter;
        _isCompiled = true; 
    }

    private static void EnsureCompiled()
    {
        if (_isCompiled) return;

        lock (_compileLock)
        {
            if (_isCompiled) return;

            
            if (TryCreateDirectAccessors(out var directSetter, out var directGetter))
            {
                _staticSetter = directSetter;
                _staticGetter = directGetter;
            }
            else
            {
                
                _staticSetter = CreateSetter();
                _staticGetter = CreateGetter();
            }

            _isCompiled = true;
        }
    }

    
    private static bool TryCreateDirectAccessors(out Action<IConnectorValue, T> setter, out Func<IConnectorValue, T> getter)
    {
        setter = null;
        getter = null;

        try
        {
            
            var field = typeof(IConnectorValue).GetField("_value",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (field != null && field.FieldType == typeof(T))
            {
                setter = (connector, value) => field.SetValue(connector, value);
                getter = (connector) => (T)field.GetValue(connector);
                return true;
            }

            
            var connectorType = typeof(IConnectorValue).Assembly.GetTypes()
                .FirstOrDefault(t => typeof(IConnectorValue).IsAssignableFrom(t) &&
                                   t.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType == typeof(T));

            if (connectorType != null)
            {
                field = connectorType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
                setter = (connector, value) => field.SetValue(connector, value);
                getter = (connector) => (T)field.GetValue(connector);
                return true;
            }
        }
        catch
        {
            
        }

        return false;
    }

    private static Action<IConnectorValue, T> CreateSetter()
    {
        var wrappedType = typeof(IConnectorValue);

        
        var setValueMethod = wrappedType.GetMethod("SetValue", BindingFlags.Public | BindingFlags.Instance);
        if (setValueMethod != null && setValueMethod.GetParameters().Length == 1)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var valueParam = Expression.Parameter(typeof(T));
            var call = Expression.Call(connectorParam, setValueMethod, valueParam);
            return Expression.Lambda<Action<IConnectorValue, T>>(call, connectorParam, valueParam).Compile();
        }

        
        var valueField = wrappedType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance) ??
                        wrappedType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance) ??
                        wrappedType.GetField("m_value", BindingFlags.NonPublic | BindingFlags.Instance);

        if (valueField != null)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var valueParam = Expression.Parameter(typeof(T));
            var fieldAccess = Expression.Field(Expression.Convert(connectorParam, wrappedType), valueField);
            var assign = Expression.Assign(fieldAccess, valueParam);
            return Expression.Lambda<Action<IConnectorValue, T>>(assign, connectorParam, valueParam).Compile();
        }

        return null;
    }

    private static Func<IConnectorValue, T> CreateGetter()
    {
        var wrappedType = typeof(IConnectorValue);

        
        var getValueMethod = wrappedType.GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance);
        if (getValueMethod != null && getValueMethod.ReturnType == typeof(T) && getValueMethod.GetParameters().Length == 0)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var call = Expression.Call(connectorParam, getValueMethod);
            return Expression.Lambda<Func<IConnectorValue, T>>(call, connectorParam).Compile();
        }

        
        var valueField = wrappedType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance) ??
                        wrappedType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance) ??
                        wrappedType.GetField("m_value", BindingFlags.NonPublic | BindingFlags.Instance);

        if (valueField != null)
        {
            var connectorParam = Expression.Parameter(typeof(IConnectorValue));
            var fieldAccess = Expression.Field(Expression.Convert(connectorParam, wrappedType), valueField);
            return Expression.Lambda<Func<IConnectorValue, T>>(fieldAccess, connectorParam).Compile();
        }

        return null;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValue(T value)
    {
        if (_staticSetter != null)
            _staticSetter(_wrappedValue, value);
        else
            FallbackSetValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue()
    {
        if (_staticGetter != null)
            return _staticGetter(_wrappedValue);
        else
            return FallbackGetValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValueFast(object value)
    {
        try
        {
            SetValue((T)Convert.ChangeType(value, typeof(T)));
        }
        catch
        {
            
            if (value is T typedValue)
                SetValue(typedValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetValueFast() => GetValue();

    
    private void FallbackSetValue(T value)
    {
        if (_wrappedValue is IFastConnectorValue<T> fastValue)
            fastValue.SetValue(value);
        else
            Debug.LogWarning($"No setter available for {typeof(T)}");
    }

    private T FallbackGetValue()
    {
        if (_wrappedValue is IFastConnectorValue<T> fastValue)
            return fastValue.GetValue();

        Debug.LogWarning($"No getter available for {typeof(T)}");
        return default;
    }
}