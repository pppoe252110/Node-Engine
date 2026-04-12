using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Split")]
public class Vector3SplitNode : BaseNode
{
    [NodePort("Vector", true)] public Vector3 vector;
    [NodePort("X", false)] public float x;
    [NodePort("Y", false)] public float y;
    [NodePort("Z", false)] public float z;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int vecId = context.GetInputId("Vector");
        int xId = context.GetOutputId("X");
        int yId = context.GetOutputId("Y");
        int zId = context.GetOutputId("Z");
        return ctx => {
            Vector3 v = ctx.Read<Vector3>(vecId);
            ctx.Write(xId, v.x);
            ctx.Write(yId, v.y);
            ctx.Write(zId, v.z);
            return ExecutionResult.Continue(-1);
        };
    }
}