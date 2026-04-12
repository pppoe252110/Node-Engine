using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Noise/Perlin Noise 3D")]
public class PerlinNoise3DNode : BaseNode
{
    [NodePort("X", true)] public float x;
    [NodePort("Y", true)] public float y;
    [NodePort("Z", true)] public float z;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int xId = context.GetInputId("X");
        int yId = context.GetInputId("Y");
        int zId = context.GetInputId("Z");
        int outId = context.GetOutputId("Result");

        return ctx =>
        {
            float vx = ctx.Read<float>(xId);
            float vy = ctx.Read<float>(yId);
            float vz = ctx.Read<float>(zId);
            ctx.Write(outId, Mathf.PerlinNoise(vx, vz));
            return ExecutionResult.Continue(-1);
        };
    }
}