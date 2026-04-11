using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Cos")]
public class CosNode : BaseNode
{
    [NodePort("Angle", true)] public float angle;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int angleId = context.GetInputId("Angle");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float a = ctx.Read<float>(angleId);
            ctx.Write(resId, Mathf.Cos(a));
            return ExecutionResult.Continue(-1);
        };
    }
}