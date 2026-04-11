using System;

[NodePath("Math/Subtract")]
public class SubtractNode : BaseNode
{
    [NodePort("A", true)] public float inputA;
    [NodePort("B", true)] public float inputB;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int aId = GetInputId("A");
        int bId = GetInputId("B");
        int resId = GetOutputId("Result");

        return (ctx) => {
            float a = Read<float>(ctx, aId);
            float b = Read<float>(ctx, bId);
            Write(ctx, resId, a - b);
            return ExecutionResult.Continue(-1);
        };
    }
}