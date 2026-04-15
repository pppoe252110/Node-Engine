using NodeEngine.Compilation;
using System;

public abstract class SubgraphInputNode<T> : SubgraphInputNodeBase
{
    [NodePort("Value", false)]
    public T valueField;

    public T Value => valueField;
    public event Action<T> OnValueChanged;
    public override event Action<object> OnUntypedValueChanged;

    public override object GetUntypedValue() => valueField;
    public override Type GetValueType() => typeof(T);

    public override void SetValueFromParentUntyped(object value)
    {
        valueField = (T)value;
        OnValueChanged?.Invoke(valueField);
        OnUntypedValueChanged?.Invoke(valueField);
    }

    public void SetValueFromParent(T value)
    {
        valueField = value;
        OnValueChanged?.Invoke(value);
        OnUntypedValueChanged?.Invoke(value);
    }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int outId = context.GetOutputId("Value");
        T val = valueField;
        return ctx =>
        {
            ctx.Write(outId, val);
            return ExecutionResult.Continue(-1);
        };
    }
}