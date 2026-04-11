using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/RadToDeg")]
public class RadToDegNode : BaseNode
{
    [NodePort("Radians", true)] public float radians;
    [NodePort("Degrees", false)] public float degrees;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int radId = context.GetInputId("Radians");
        int degId = context.GetOutputId("Degrees");

        return (ctx) => {
            float r = ctx.Read<float>(radId);
            ctx.Write(degId, r * Mathf.Rad2Deg);
            return ExecutionResult.Continue(-1);
        };
    }
}