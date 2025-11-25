using System;
using UnityEngine;

[Serializable]
public class ConnectorValueBool : ConnectorValueBase, IFastConnectorValue<bool>, IConnectorValueBridge
{
    [SerializeField] private bool _value;

    public ConnectorValueBool() { }
    public ConnectorValueBool(bool value) => _value = value;

    
    public void SetValue(bool value) => _value = value;
    public bool GetValue() => _value;
    public override object GetInnerValue() => _value;

    
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(bool);
    public void SetValueFast(object value) => _value = (bool)value;
    public object GetValueFast() => _value;

    
    public ConnectorValueBool And(bool other) => new ConnectorValueBool(_value && other);
    public ConnectorValueBool Or(bool other) => new ConnectorValueBool(_value || other);
    public ConnectorValueBool Not() => new ConnectorValueBool(!_value);
    public ConnectorValueBool Xor(bool other) => new ConnectorValueBool(_value ^ other);

    
    public ConnectorValueInt ToInt() => new ConnectorValueInt(_value ? 1 : 0);

    
    public static implicit operator bool(ConnectorValueBool connector) => connector._value;

    
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<bool>(this);
    }
}