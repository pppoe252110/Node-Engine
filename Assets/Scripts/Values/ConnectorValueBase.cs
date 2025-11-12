[System.Serializable]
public class ConnectorValueBase<T> : IConnectorValue
{
    private T _value;

    public delegate void Invoke(T value);
    public Invoke ValueUpdated;

    // Add the interface event
    event System.Action<object> IConnectorValue.ValueUpdated
    {
        add { ValueUpdated += (v) => value(v); }
        remove { ValueUpdated -= (v) => value(v); }
    }

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

    public virtual void ProceedValue()
    {
    }

    public object GetInnerValue()
    {
        return _value;
    }
}