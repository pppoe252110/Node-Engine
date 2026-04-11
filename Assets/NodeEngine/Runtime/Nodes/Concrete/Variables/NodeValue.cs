using System;
using UnityEngine;

[Serializable]
public class NodeValue
{
    [SerializeReference]
    private object _value;

    public NodeValue(object defaultValue = null) => _value = defaultValue;

    public object GetInnerValue() => _value;

    public void SetInnerValue(object newValue)
    {
        _value = newValue;
    }
}