public class ConnectorValueInt : IConnectorValue, IConnectorValue<int>
{
    public int Value;

    public ConnectorValueInt(int value = 0) { Value = value; }

    public int GetInnerValue() => Value;
    public void SetInnerValue(int value) => Value = value;

    object IConnectorValue.GetInnerValue() => Value;
    void IConnectorValue.SetInnerValue(object value) => Value = (int)value;

    // Direct access methods for performance
    public int GetValueDirect() => Value;
    public void SetValueDirect(int value) => Value = value;
}