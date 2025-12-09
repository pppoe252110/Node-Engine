using System;
using UnityEngine;

public class ConnectorValueType : IConnectorValue, IConnectorValue<Type>
{
    public Type Value;
    public ConnectorValueType(Type value = null) { Value = value; }
    public Type GetInnerValue() => Value;
    public void SetInnerValue(Type value) => Value = value;
    object IConnectorValue.GetInnerValue() => Value;
    void IConnectorValue.SetInnerValue(object value) => Value = (Type)value;
    public Type InnerType => typeof(Type);
}
