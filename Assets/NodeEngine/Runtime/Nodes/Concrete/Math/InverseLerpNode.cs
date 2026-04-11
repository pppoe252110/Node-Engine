using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/InverseLerp")]
public class InverseLerpNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("Value", true)] public float value;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int valId = context.GetInputId("Value");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float aVal = Read<float>(ctx, aId);
            float bVal = Read<float>(ctx, bId);
            float v = Read<float>(ctx, valId);
            Write(ctx, resId, Mathf.InverseLerp(aVal, bVal, v));
            return ExecutionResult.Continue(-1);
        };
    }
}