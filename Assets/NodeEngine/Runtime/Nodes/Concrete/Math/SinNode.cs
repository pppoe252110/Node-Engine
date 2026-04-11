using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Sin")]
public class SinNode : BaseNode
{
    [NodePort("Angle", true)] public float angle; // in radians
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int angleId = context.GetInputId("Angle");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float a = Read<float>(ctx, angleId);
            Write(ctx, resId, Mathf.Sin(a));
            return ExecutionResult.Continue(-1);
        };
    }
}