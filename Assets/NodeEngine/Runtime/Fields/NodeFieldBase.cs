using System;

public abstract class NodeFieldBase
{
    public Connector Connector { get; set; }
    public abstract Type GetValueType();
    public abstract void ProceedValue();
    public abstract NodeValueAttribute GetAttribute();
    public abstract object GetObjectValue(); // <-- This must be here
    public abstract void UpdateValueFromSource(IConnectorValue sourceValue);
}

public abstract class NodeFieldBase<T> : NodeFieldBase
{
    // This is the strongly-typed value managed by the field.
    protected T _currentValue;

    public override Type GetValueType() => typeof(T);
    public abstract T GetValue();
}