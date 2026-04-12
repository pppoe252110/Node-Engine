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
            float yVal = ctx.Read<float>(yId);
            float xVal = ctx.Read<float>(xId);
            ctx.Write(resId, Mathf.Atan2(yVal, xVal));
            return ExecutionResult.Continue(-1);
        };
    }
}