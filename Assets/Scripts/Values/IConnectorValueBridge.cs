using System;

public interface IConnectorValueBridge
{
    IConnectorValue WrappedValue { get; }
    Type ValueType { get; }
    void SetValueFast(object value);
    object GetValueFast();
}

public interface ITypedConnectorBridge<T> : IConnectorValueBridge
{
    void SetValue(T value);
    T GetValue();
}