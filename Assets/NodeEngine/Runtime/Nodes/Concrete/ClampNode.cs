using System;

[NodePath("Math/Clamp")]
public class ClampNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Min", true)] public float min;
    [NodePort("Max", true)] public float max;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int valId = GetInputId("Value");
        int minId = GetInputId("Min");
        int maxId = GetInputId("Max");
        int resultId = GetOutputId("Result");

        return (ctx) => {
            float v = Read<float>(ctx, valId);
            float mn = Read<float>(ctx, minId);
            float mx = Read<float>(ctx, maxId);
            Write(ctx, resultId, Math.Clamp(v, mn, mx));
            return ExecutionResult.Continue(-1);
        };
    }
}