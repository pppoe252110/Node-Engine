using NodeEngine.Compilation;
using System;

[NodePath("Math/Clamp")]
public class ClampNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Min", true)] public float min;
    [NodePort("Max", true)] public float max;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int minId = context.GetInputId("Min");
        int maxId = context.GetInputId("Max");
        int resultId = context.GetOutputId("Result");

        return (ctx) => {
            float v = ctx.Read<float>(valId);
            float mn = ctx.Read<float>(minId);
            float mx = ctx.Read<float>(maxId);
            ctx.Write(resultId, Math.Clamp(v, mn, mx));
            return ExecutionResult.Continue(-1);
        };
    }
}