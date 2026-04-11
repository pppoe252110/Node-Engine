using NodeEngine.Compilation;
using System;

[NodePath("Math/Sign")]
public class SignNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Result", false)] public int result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float v = Read<float>(ctx, valId);
            Write(ctx, resId, Math.Sign(v));
            return ExecutionResult.Continue(-1);
        };
    }
}