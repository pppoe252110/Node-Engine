using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/DegToRad")]
public class DegToRadNode : BaseNode
{
    [NodePort("Degrees", true)] public float degrees;
    [NodePort("Radians", false)] public float radians;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int degId = context.GetInputId("Degrees");
        int radId = context.GetOutputId("Radians");

        return (ctx) => {
            float d = ctx.Read<float>(degId);
            ctx.Write(radId, d * Mathf.Deg2Rad);
            return ExecutionResult.Continue(-1);
        };
    }
}