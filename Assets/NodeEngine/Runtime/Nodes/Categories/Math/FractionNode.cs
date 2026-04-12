using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Fraction")]
public class FractionNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float v = ctx.Read<float>(valId);
            ctx.Write(outId, v - Mathf.Floor(v));
            return ExecutionResult.Continue(-1);
        };
    }
}