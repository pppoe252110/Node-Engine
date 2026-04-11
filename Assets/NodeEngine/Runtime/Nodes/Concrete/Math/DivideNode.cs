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
            float a = Read<float>(ctx, aId);
            float b = Read<float>(ctx, bId);
            Write(ctx, resId, b != 0 ? a / b : 0f);
            return ExecutionResult.Continue(-1);
        };
    }
}