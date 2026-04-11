using System;
using UnityEngine;

[NodePath("Math/RadToDeg")]
public class RadToDegNode : BaseNode
{
    [NodePort("Radians", true)] public float radians;
    [NodePort("Degrees", false)] public float degrees;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int radId = GetInputId("Radians");
        int degId = GetOutputId("Degrees");

        return (ctx) => {
            float r = Read<float>(ctx, radId);
            Write(ctx, degId, r * Mathf.Rad2Deg);
            return ExecutionResult.Continue(-1);
        };
    }
}