// Values\ConnectorValueInt.cs
using System;
using UnityEngine;

[Serializable]
public class ConnectorValueInt : ConnectorValueBase, IFastConnectorValue<int>, IConnectorValueBridge
{
    [SerializeField] private int _value;

    public ConnectorValueInt() { }
    public ConnectorValueInt(int value) => _value = value;

    // Fast interface implementation
    public void SetValue(int value) => _value = value;
    public int GetValue() => _value;
    public override object GetInnerValue() => _value;

    // Bridge interface implementation (self-bridging)
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(int);
    public void SetValueFast(object value) => _value = (int)value;
    public object GetValueFast() => _value;

    // Math operations
    public ConnectorValueInt Add(int other) => new ConnectorValueInt(_value + other);
    public ConnectorValueInt Subtract(int other) => new ConnectorValueInt(_value - other);
    public ConnectorValueInt Multiply(int other) => new ConnectorValueInt(_value * other);
    public ConnectorValueInt Divide(int other) => new ConnectorValueInt(_value / other);

    // Comparison
    public bool GreaterThan(int other) => _value > other;
    public bool LessThan(int other) => _value < other;
    public bool Equals(int other) => _value == other;

    // Override bridge creation for optimal performance
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<int>(this);
    }
}