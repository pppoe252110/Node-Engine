public class ConnectorValueBase<T> : IConnectorValue
{
    private T _value;

    public delegate void Invoke(T value);
    public Invoke ValueUpdated;

    public ConnectorValueBase(T value)
    {
        SetValue(value);
    }

    public virtual void SetValue(T value)
    {
        _value = value;
        ValueUpdated?.Invoke(value);
    }

    public virtual T GetValue()
    {
        return _value;
    }

}
