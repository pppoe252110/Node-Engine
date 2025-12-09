using System;

public class ConnectorValueString : IConnectorValue, IConnectorValue<string>
{
    private string _value;

    public ConnectorValueString(string value = "")
    {
        _value = value;
    }

    public string GetInnerValue() => _value;
    object IConnectorValue.GetInnerValue() => _value;

    public void SetInnerValue(string value) => _value = value;
    void IConnectorValue.SetInnerValue(object value) => _value = (string)value;
    public Type InnerType => typeof(string);
}
