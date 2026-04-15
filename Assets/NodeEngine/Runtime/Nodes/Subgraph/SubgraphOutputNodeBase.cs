using System;

public abstract class SubgraphOutputNodeBase : BaseNode
{
    public virtual object GetUntypedValue() => null;
    public virtual Type GetValueType() => typeof(void);
}