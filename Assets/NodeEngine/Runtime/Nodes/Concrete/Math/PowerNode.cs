using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Power")]
public class PowerNode : BaseNode
{
    [NodePort("Base", true)] public float @base;
    [NodePort("Exponent", true)] public float exponent;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int baseId = context.GetInputId("Base");
        int expId = context.GetInputId("Exponent");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float b = ctx.Read<float>(baseId);
            float e = ctx.Read<float>(expId);
            ctx.Write(resId, Mathf.Pow(b, e));
            return ExecutionResult.Continue(-1);
        };
    }
}