using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Exponential")]
public class ExpNode : BaseNode
{
    [NodePort("Power", true)] public float power;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int pwrId = context.GetInputId("Power");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float p = ctx.Read<float>(pwrId);
            ctx.Write(outId, Mathf.Exp(p));
            return ExecutionResult.Continue(-1);
        };
    }
}