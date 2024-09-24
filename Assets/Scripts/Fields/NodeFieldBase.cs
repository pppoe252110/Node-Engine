using System;

public abstract class NodeFieldBase
{
    public abstract Type GetValueType();
    public abstract void ProceedValue();
    public abstract NodeValueAttribute GetAttribute();
}