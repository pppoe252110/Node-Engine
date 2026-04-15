using NodeEngine.Compilation;
using System;

public abstract class SubgraphOutputNode<T> : SubgraphOutputNodeBase
{
    [NodePort("Value", true)]
    public T valueField;

    public T Value => valueField;
    public override object GetUntypedValue() => valueField;
    public override Type GetValueType() => typeof(T);

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int inId = context.GetInputId("Value");
        return ctx =>
        {
            valueField = ctx.Read<T>(inId);
            return ExecutionResult.Stop();
        };
    }
}