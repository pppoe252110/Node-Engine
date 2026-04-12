using NodeEngine.Compilation;
using System;

[NodePath("Math/Modulo")]
public class ModuloNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float aVal = ctx.Read<float>(aId);
            float bVal = ctx.Read<float>(bId);
            ctx.Write(resId, aVal % bVal);
            return ExecutionResult.Continue(-1);
        };
    }
}