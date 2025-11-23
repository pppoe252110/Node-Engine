using System;
using UnityEngine;

[Serializable]
public class ConnectorValueBool : ConnectorValueBase, IFastConnectorValue<bool>, IConnectorValueBridge
{
    [SerializeField] private bool _value;

    public ConnectorValueBool() { }
    public ConnectorValueBool(bool value) => _value = value;

    // Fast interface implementation
    public void SetValue(bool value) => _value = value;
    public bool GetValue() => _value;
    public override object GetInnerValue() => _value;

    // Bridge interface implementation (self-bridging)
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(bool);
    public void SetValueFast(object value) => _value = (bool)value;
    public object GetValueFast() => _value;

    // Logical operations
    public ConnectorValueBool And(bool other) => new ConnectorValueBool(_value && other);
    public ConnectorValueBool Or(bool other) => new ConnectorValueBool(_value || other);
    public ConnectorValueBool Not() => new ConnectorValueBool(!_value);
    public ConnectorValueBool Xor(bool other) => new ConnectorValueBool(_value ^ other);

    // Utility
    public ConnectorValueInt ToInt() => new ConnectorValueInt(_value ? 1 : 0);

    // Conversion
    public static implicit operator bool(ConnectorValueBool connector) => connector._value;

    // Override bridge creation for optimal performance
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<bool>(this);
    }
}