using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Logarithm")]
public class LogarithmNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Base", true)] public float baseValue = (float)Math.E;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int baseId = context.GetInputId("Base");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float v = ctx.Read<float>(valId);
            float b = ctx.Read<float>(baseId, (float)Math.E);
            ctx.Write(outId, Mathf.Log(v, b));
            return ExecutionResult.Continue(-1);
        };
    }
}