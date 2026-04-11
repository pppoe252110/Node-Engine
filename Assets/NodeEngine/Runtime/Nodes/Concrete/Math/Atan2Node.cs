using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Atan2")]
public class Atan2Node : BaseNode
{
    [NodePort("Y", true)] public float y;
    [NodePort("X", true)] public float x;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int yId = context.GetInputId("Y");
        int xId = context.GetInputId("X");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float yVal = Read<float>(ctx, yId);
            float xVal = Read<float>(ctx, xId);
            Write(ctx, resId, Mathf.Atan2(yVal, xVal));
            return ExecutionResult.Continue(-1);
        };
    }
}