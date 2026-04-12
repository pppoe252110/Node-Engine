using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Noise/Perlin Noise 2D")]
public class PerlinNoise2DNode : BaseNode
{
    [NodePort("X", true)] public float x;
    [NodePort("Y", true)] public float y;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int xId = context.GetInputId("X");
        int yId = context.GetInputId("Y");
        int outId = context.GetOutputId("Result");

        return ctx =>
        {
            float xVal = ctx.Read<float>(xId);
            float yVal = ctx.Read<float>(yId);
            ctx.Write(outId, Mathf.PerlinNoise(xVal, yVal));
            return ExecutionResult.Continue(-1);
        };
    }
}