using System;

public abstract class SubgraphInputNodeBase : BaseNode, IVariableNode
{
    public virtual void SetValueFromParentUntyped(object value) { }
    public virtual object GetUntypedValue() => null;
    public virtual Type GetValueType() => typeof(void);
    public virtual event Action<object> OnUntypedValueChanged;
    object IVariableNode.GetUntypedValue() => GetUntypedValue();
    void IVariableNode.SetUntypedValue(object value) => SetValueFromParentUntyped(value);
    Type IVariableNode.ValueType => GetValueType();
}