using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Lerp")]
public class LerpNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("T", true)] public float t;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int tId = context.GetInputId("T");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float aVal = ctx.Read<float>(aId);
            float bVal = ctx.Read<float>(bId);
            float tVal = ctx.Read<float>(tId);
            ctx.Write(resId, Mathf.Lerp(aVal, bVal, tVal));
            return ExecutionResult.Continue(-1);
        };
    }
}