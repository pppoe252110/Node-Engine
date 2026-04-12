using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Rotate Vector")]
public class RotateVectorNode : BaseNode
{
    [NodePort("Vector", true)] public Vector3 vector;
    [NodePort("Rotation", true)] public Quaternion rotation;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int vecId = context.GetInputId("Vector");
        int rotId = context.GetInputId("Rotation");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 v = ctx.Read<Vector3>(vecId);
            Quaternion r = ctx.Read<Quaternion>(rotId);
            ctx.Write(outId, r * v);
            return ExecutionResult.Continue(-1);
        };
    }
}