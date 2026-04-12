using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Snap To Grid")]
public class SnapToGridNode : BaseNode
{
    [NodePort("Value", true)] public float value;
    [NodePort("Grid Size", true)] public float gridSize;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int valId = context.GetInputId("Value");
        int gridId = context.GetInputId("Grid Size");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float v = ctx.Read<float>(valId);
            float g = ctx.Read<float>(gridId);
            ctx.Write(outId, Mathf.Round(v / g) * g);
            return ExecutionResult.Continue(-1);
        };
    }
}