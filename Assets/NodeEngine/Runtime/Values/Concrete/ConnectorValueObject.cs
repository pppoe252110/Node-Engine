using System;
using UnityEngine;

[Serializable]
public class ConnectorValueObject : ConnectorValueBase<object>
{
    [SerializeField] private Type _storedType;
    public ConnectorValueObject() { _value = null; _storedType = typeof(object); }
    public ConnectorValueObject(object value) { SetValue(value); }
    public void SetValue(object value) { _value = value; _storedType = value?.GetType() ?? typeof(object); }
    public object GetValue() => _value;
    public override object GetInnerValue() => _value;
    public override Type InnerType => _storedType;
}