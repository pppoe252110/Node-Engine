using System;

public interface IConnectorValue
{
    object GetInnerValue();
    void SetInnerValue(object value);
    Type InnerType { get; } 
}

public interface IConnectorValue<T> : IConnectorValue
{
    new T GetInnerValue();
    void SetInnerValue(T value);
}