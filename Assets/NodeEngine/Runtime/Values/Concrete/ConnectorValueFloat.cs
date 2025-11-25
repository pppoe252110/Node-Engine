using System;
using UnityEngine;

[Serializable]
public class ConnectorValueFloat : ConnectorValueBase, IFastConnectorValue<float>, IConnectorValueBridge
{
    [SerializeField] private float _value;

    public ConnectorValueFloat() { }
    public ConnectorValueFloat(float value) => _value = value;

    
    public void SetValue(float value) => _value = value;
    public float GetValue() => _value;
    public override object GetInnerValue() => _value;

    
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(float);
    public void SetValueFast(object value) => _value = (float)value;
    public object GetValueFast() => _value;

    
    public ConnectorValueFloat Add(float other) => new ConnectorValueFloat(_value + other);
    public ConnectorValueFloat Subtract(float other) => new ConnectorValueFloat(_value - other);
    public ConnectorValueFloat Multiply(float other) => new ConnectorValueFloat(_value * other);
    public ConnectorValueFloat Divide(float other) => new ConnectorValueFloat(_value / other);

    
    public ConnectorValueInt Round() => new ConnectorValueInt(Mathf.RoundToInt(_value));
    public ConnectorValueInt Floor() => new ConnectorValueInt(Mathf.FloorToInt(_value));
    public ConnectorValueInt Ceil() => new ConnectorValueInt(Mathf.CeilToInt(_value));

    
    public bool Approximately(float other, float tolerance = 0.0001f) => Mathf.Abs(_value - other) < tolerance;

    
    public static implicit operator float(ConnectorValueFloat connector) => connector._value;

    
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<float>(this);
    }
}