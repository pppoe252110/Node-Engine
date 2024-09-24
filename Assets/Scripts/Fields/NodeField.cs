using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class NodeField<T> : NodeFieldBase where T : IConnectorValue
{
    public delegate void ValueHandlerFunc(T value);
    public event ValueHandlerFunc CurrentValueHandler;
    private T currentValue;

    public NodeField<T> SetFunc(ValueHandlerFunc value)
    {
        CurrentValueHandler = value;
        return this;
    }

    public NodeField<T> ProvideDefaultValue(T value)
    {
        currentValue = value;
        return this;
    }

    public override NodeValueAttribute GetAttribute()
    {
        return CurrentValueHandler.GetMethodInfo().GetCustomAttribute<NodeValueAttribute>();
    }
    public IConnectorValue value;
    public override Type GetValueType()
    {
        return CurrentValueHandler.GetType();
    }

    public override void ProceedValue()
    {
        CurrentValueHandler.Invoke(currentValue);
    }
}
