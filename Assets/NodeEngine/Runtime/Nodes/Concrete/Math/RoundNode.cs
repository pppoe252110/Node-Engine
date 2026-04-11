using System;
using UnityEngine;

[NodePath("Math/Round")]
public class RoundNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int valId = GetInputId("Value");
        int resId = GetOutputId("Result");

        return (ctx) => {
            float v = Read<float>(ctx, valId);
            Write(ctx, resId, Mathf.Round(v));
            return ExecutionResult.Continue(-1);
        };
    }
}