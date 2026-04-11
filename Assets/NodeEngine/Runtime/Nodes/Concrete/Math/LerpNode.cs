using System;
using UnityEngine;

[NodePath("Math/Lerp")]
public class LerpNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("T", true)] public float t;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int aId = GetInputId("A");
        int bId = GetInputId("B");
        int tId = GetInputId("T");
        int resId = GetOutputId("Result");

        return (ctx) => {
            float aVal = Read<float>(ctx, aId);
            float bVal = Read<float>(ctx, bId);
            float tVal = Read<float>(ctx, tId);
            Write(ctx, resId, Mathf.Lerp(aVal, bVal, tVal));
            return ExecutionResult.Continue(-1);
        };
    }
}