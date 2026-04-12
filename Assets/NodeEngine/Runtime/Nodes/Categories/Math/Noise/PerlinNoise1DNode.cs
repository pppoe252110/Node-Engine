using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Noise/Perlin Noise 1D")]
public class PerlinNoise1DNode : BaseNode
{
    [NodePort("X", true)] public float x;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int xId = context.GetInputId("X");
        int outId = context.GetOutputId("Result");

        return ctx =>
        {
            float xVal = ctx.Read<float>(xId);
            // Use fixed Y = 0 to get 1D noise
            ctx.Write(outId, Mathf.PerlinNoise(xVal, 0f));
            return ExecutionResult.Continue(-1);
        };
    }
}