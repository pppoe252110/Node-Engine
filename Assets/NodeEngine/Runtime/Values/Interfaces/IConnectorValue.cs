// Core interfaces
public interface IConnectorValue
{
    object GetInnerValue();
}

public interface IFastConnectorValue<T> : IConnectorValue
{
    void SetValue(T value);
    T GetValue();
}
