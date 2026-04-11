using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Floor")]
public class FloorNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float v = ctx.Read<float>(valId);
            ctx.Write(resId, Mathf.Floor(v));
            return ExecutionResult.Continue(-1);
        };
    }
}
