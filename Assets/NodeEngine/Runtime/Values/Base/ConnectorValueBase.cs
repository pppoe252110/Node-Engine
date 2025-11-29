public abstract class ConnectorValueBase<T> : IConnectorValue, IConnectorValue<T>
{
    protected T _value;

    protected ConnectorValueBase(T defaultValue = default(T))
    {
        _value = defaultValue;
    }

    public virtual T GetInnerValue() => _value;
    public virtual void SetInnerValue(T value) => _value = value;

    object IConnectorValue.GetInnerValue() => _value;
    void IConnectorValue.SetInnerValue(object value) => _value = (T)value;
}
