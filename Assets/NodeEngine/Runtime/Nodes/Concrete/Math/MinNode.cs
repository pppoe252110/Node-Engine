using System;
using UnityEngine;

[NodePath("Math/Min")]
public class MinNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int aId = GetInputId("A");
        int bId = GetInputId("B");
        int resId = GetOutputId("Result");

        return (ctx) => {
            float aVal = Read<float>(ctx, aId);
            float bVal = Read<float>(ctx, bId);
            Write(ctx, resId, Mathf.Min(aVal, bVal));
            return ExecutionResult.Continue(-1);
        };
    }
}