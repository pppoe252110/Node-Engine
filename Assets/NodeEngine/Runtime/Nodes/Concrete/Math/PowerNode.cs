using System;
using UnityEngine;

[NodePath("Math/Power")]
public class PowerNode : BaseNode
{
    [NodePort("Base", true)] public float @base;
    [NodePort("Exponent", true)] public float exponent;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int baseId = GetInputId("Base");
        int expId = GetInputId("Exponent");
        int resId = GetOutputId("Result");

        return (ctx) => {
            float b = Read<float>(ctx, baseId);
            float e = Read<float>(ctx, expId);
            Write(ctx, resId, Mathf.Pow(b, e));
            return ExecutionResult.Continue(-1);
        };
    }
}