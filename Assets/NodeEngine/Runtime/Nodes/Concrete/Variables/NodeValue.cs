using System;

[Serializable]
public class NodeValue
{
    // Serialized so Unity can save the value set in the Inspector/UI
    private object _value;

    public NodeValue(object defaultValue = null) => _value = defaultValue;

    public object GetInnerValue() => _value;

    public void SetInnerValue(object newValue)
    {
        _value = newValue;
    }
}