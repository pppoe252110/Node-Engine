public interface IConnectorValue
{
    object GetInnerValue();
    void SetInnerValue(object value);
}

public interface IConnectorValue<T> : IConnectorValue
{
    new T GetInnerValue();
    void SetInnerValue(T value);
}
