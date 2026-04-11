using System;
using UnityEngine;

[NodePath("Math/Cos")]
public class CosNode : BaseNode
{
    [NodePort("Angle", true)] public float angle;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int angleId = GetInputId("Angle");
        int resId = GetOutputId("Result");

        return (ctx) => {
            float a = Read<float>(ctx, angleId);
            Write(ctx, resId, Mathf.Cos(a));
            return ExecutionResult.Continue(-1);
        };
    }
}