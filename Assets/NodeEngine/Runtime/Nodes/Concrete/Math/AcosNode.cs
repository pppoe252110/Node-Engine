using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Acos")]
public class AcosNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int resId = context.GetOutputId("Result");

        return (ctx) => {
            float v = Read<float>(ctx, valId);
            Write(ctx, resId, Mathf.Acos(v));
            return ExecutionResult.Continue(-1);
        };
    }
}