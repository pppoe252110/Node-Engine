using System;
using UnityEngine;

public abstract class NodeFieldBase : MonoBehaviour
{
    public BaseNode OwnerNode { get; private set; }
    public string PortName { get; private set; }

    public virtual void Initialize(BaseNode owner, string portName)
    {
        OwnerNode = owner;
        PortName = portName;
    }

    public abstract object GetValue();
    public abstract void SetValue(object value);
}