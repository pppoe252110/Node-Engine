using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Create")]
public class Vector3CreateNode : BaseNode
{
    [NodePort("X", true)] public float x;
    [NodePort("Y", true)] public float y;
    [NodePort("Z", true)] public float z;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int xId = context.GetInputId("X");
        int yId = context.GetInputId("Y");
        int zId = context.GetInputId("Z");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float vx = ctx.Read<float>(xId);
            float vy = ctx.Read<float>(yId);
            float vz = ctx.Read<float>(zId);
            ctx.Write(outId, new Vector3(vx, vy, vz));
            return ExecutionResult.Continue(-1);
        };
    }
}