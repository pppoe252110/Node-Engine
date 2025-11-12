using System;

public abstract class NodeFieldBase
{
    public Connector Connector { get; set; }
    public abstract Type GetValueType();
    public abstract void ProceedValue();
    public abstract NodeValueAttribute GetAttribute();
    public abstract object GetObjectValue();
}