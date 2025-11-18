using System;
using UnityEngine;

// Base class for all connector values (your existing hierarchy)
public abstract class ConnectorValueBase : IConnectorValue
{
    public abstract object GetInnerValue();

    // Fast bridge access
    private IConnectorValueBridge _fastBridge;

    public IConnectorValueBridge GetFastBridge()
    {
        if (_fastBridge == null)
        {
            _fastBridge = CreateFastBridge();
        }
        return _fastBridge;
    }

    protected virtual IConnectorValueBridge CreateFastBridge()
    {
        var valueType = GetInnerValue()?.GetType() ?? typeof(object);
        var bridgeType = typeof(FastConnectorBridge<>).MakeGenericType(valueType);
        return (IConnectorValueBridge)Activator.CreateInstance(bridgeType, this);
    }
}

// Generic base for typed values
public abstract class ConnectorValue<T> : ConnectorValueBase, IFastConnectorValue<T>, IConnectorValueBridge
{
    [SerializeField] protected T _value;

    // IConnectorValue implementation
    public override object GetInnerValue() => _value;

    // IFastConnectorValue<T> implementation
    public virtual void SetValue(T value) => _value = value;
    public virtual T GetValue() => _value;

    // IConnectorValueBridge implementation
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(T);
    public void SetValueFast(object value)
    {
        if (value is T typedValue)
            SetValue(typedValue);
    }
    public object GetValueFast() => _value;

    // Operator overloads for convenience
    public static implicit operator T(ConnectorValue<T> connector) => connector._value;

    public override string ToString() => _value?.ToString() ?? "null";
    public override bool Equals(object obj) => obj is ConnectorValue<T> other && Equals(_value, other._value);
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}