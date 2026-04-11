using NodeEngine.Compilation;
using System;

[NodePath("Math/Divide")]
public class DivideNode : BaseNode
{
    [NodePort("A", true)] public float inputA;
    [NodePort("B", true)] public float inputB;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float a = ctx.Read<float>(aId);
            float b = ctx.Read<float>(bId);
            ctx.Write(resId, b != 0 ? a / b : 0f);
            return ExecutionResult.Continue(-1);
        };
    }
}