
using System;
using UnityEngine;

[Serializable]
public class ConnectorValueObject : ConnectorValueBase<object>
{
    [SerializeField] private Type _storedType;

    public ConnectorValueObject()
    {
        _value = null;
        _storedType = typeof(object);
    }

    public ConnectorValueObject(object value)
    {
        SetValue(value);
    }

    
    public void SetValue(object value)
    {
        _value = value;
        _storedType = value?.GetType() ?? typeof(object);
    }

    public object GetValue() => _value;
    public override object GetInnerValue() => _value;

    
    public IConnectorValue WrappedValue => this;
    public Type ValueType => _storedType;
    public void SetValueFast(object value) => SetValue(value);
    public object GetValueFast() => _value;

    
    public Type GetStoredType() => _storedType;
    public bool IsNull => _value == null;
    public bool IsValueType => _storedType?.IsValueType ?? false;

    
    public bool IsType<T>() => _value is T;
    public bool IsType(Type type) => type?.IsAssignableFrom(_storedType) ?? false;

    public bool TryGetValue<T>(out T result)
    {
        if (_value is T typedValue)
        {
            result = typedValue;
            return true;
        }

        try
        {
            result = (T)_value;
            return true;
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    
    public T GetValueAs<T>() where T : class => _value as T;
    public T GetValueCast<T>() => (T)_value;

    public override string ToString() => _value?.ToString() ?? "null";
    public override bool Equals(object obj) => obj is ConnectorValueObject other && Equals(_value, other._value);
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}