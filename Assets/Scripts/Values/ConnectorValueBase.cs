[System.Serializable]
public class ConnectorValueBase<T> : IConnectorValue
{
    private T _value;

    public delegate void Invoke(T value);
    public Invoke ValueUpdated;

    event System.Action<object> IConnectorValue.ValueUpdated
    {
        add { ValueUpdated += (v) => value(v); }
        remove { ValueUpdated -= (v) => value(v); }
    }

    public ConnectorValueBase(T value)
    {
        _value = value; // Don't trigger event in constructor
    }

    public virtual void SetValue(T value)
    {
        if (Equals(_value, value)) return; // Don't trigger if value didn't change

        _value = value;
        ValueUpdated?.Invoke(value);
    }

    public virtual T GetValue()
    {
        return _value;
    }

    public virtual void ProceedValue()
    {
        // Trigger update to propagate the current value
        ValueUpdated?.Invoke(_value);
    }

    public object GetInnerValue()
    {
        return _value;
    }
}