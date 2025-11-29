public interface IConnectorValue
{
    object GetInnerValue();
    void SetInnerValue(object value);
}

public interface IConnectorValue<out T> : IConnectorValue
{
    new T GetInnerValue();
}
