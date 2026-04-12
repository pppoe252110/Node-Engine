using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Interpolation/Smooth Step")]
public class SmoothStepNode : BaseNode
{
    [NodePort("Edge0", true)] public float edge0;
    [NodePort("Edge1", true)] public float edge1;
    [NodePort("T", true)] public float t;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int e0Id = context.GetInputId("Edge0");
        int e1Id = context.GetInputId("Edge1");
        int tId = context.GetInputId("T");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float e0 = ctx.Read<float>(e0Id);
            float e1 = ctx.Read<float>(e1Id);
            float tv = ctx.Read<float>(tId);
            ctx.Write(outId, Mathf.SmoothStep(e0, e1, tv));
            return ExecutionResult.Continue(-1);
        };
    }
}