using System;
using UnityEngine;

[Serializable]
public class ConnectorValueString : ConnectorValueBase, IFastConnectorValue<string>, IConnectorValueBridge
{
    [SerializeField] private string _value;

    public ConnectorValueString() => _value = string.Empty;
    public ConnectorValueString(string value) => _value = value ?? string.Empty;

    // Fast interface implementation
    public void SetValue(string value) => _value = value ?? string.Empty;
    public string GetValue() => _value;
    public override object GetInnerValue() => _value;

    // Bridge interface implementation (self-bridging)
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(string);
    public void SetValueFast(object value) => _value = value?.ToString() ?? string.Empty;
    public object GetValueFast() => _value;

    // String operations
    public ConnectorValueString Concat(string other) => new ConnectorValueString(_value + other);
    public ConnectorValueString ToUpper() => new ConnectorValueString(_value.ToUpper());
    public ConnectorValueString ToLower() => new ConnectorValueString(_value.ToLower());
    public ConnectorValueString Trim() => new ConnectorValueString(_value.Trim());

    // Query methods
    public bool Contains(string substring) => _value.Contains(substring);
    public bool StartsWith(string prefix) => _value.StartsWith(prefix);
    public bool EndsWith(string suffix) => _value.EndsWith(suffix);
    public ConnectorValueInt Length() => new ConnectorValueInt(_value.Length);

    // Conversion
    public bool TryParseInt(out ConnectorValueInt result)
    {
        if (int.TryParse(_value, out int intValue))
        {
            result = new ConnectorValueInt(intValue);
            return true;
        }
        result = null;
        return false;
    }

    public bool TryParseFloat(out ConnectorValueFloat result)
    {
        if (float.TryParse(_value, out float floatValue))
        {
            result = new ConnectorValueFloat(floatValue);
            return true;
        }
        result = null;
        return false;
    }

    // Conversion
    public static implicit operator string(ConnectorValueString connector) => connector._value;

    // Override bridge creation for optimal performance
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<string>(this);
    }
}