using System;

public abstract class NodeFieldBase
{
    public Connector Connector { get; set; }
    public abstract Type GetValueType();
    public abstract void ProceedValue();
    public abstract NodeValueAttribute GetAttribute();
    public abstract void UpdateValueFromSource(IConnectorValue sourceValue);
    public abstract IConnectorValue GetCurrentValue();

    public virtual bool TryUpdateOutputType(Type newType)
    {
        return false;
    }
}

public abstract class NodeFieldBase<T> : NodeFieldBase where T : IConnectorValue
{
    protected T _currentValue;

    public override Type GetValueType() => _currentValue?.InnerType ?? typeof(object);
    public abstract T GetValue();
    public override IConnectorValue GetCurrentValue() => _currentValue;
}