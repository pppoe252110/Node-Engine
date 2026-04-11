using System;
using UnityEngine;

[NodePath("Math/DegToRad")]
public class DegToRadNode : BaseNode
{
    [NodePort("Degrees", true)] public float degrees;
    [NodePort("Radians", false)] public float radians;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int degId = GetInputId("Degrees");
        int radId = GetOutputId("Radians");

        return (ctx) => {
            float d = Read<float>(ctx, degId);
            Write(ctx, radId, d * Mathf.Deg2Rad);
            return ExecutionResult.Continue(-1);
        };
    }
}