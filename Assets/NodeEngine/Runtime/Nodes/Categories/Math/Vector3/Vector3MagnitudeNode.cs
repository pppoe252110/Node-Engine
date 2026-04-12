using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Magnitude")]
public class Vector3MagnitudeNode : BaseNode
{
    [NodePort("Vector", true)] public Vector3 vector;
    [NodePort("Magnitude", false)] public float magnitude;
    [NodePort("Sqr Magnitude", false)] public float sqrMagnitude;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int vecId = context.GetInputId("Vector");
        int magId = context.GetOutputId("Magnitude");
        int sqrId = context.GetOutputId("Sqr Magnitude");
        return ctx => {
            Vector3 v = ctx.Read<Vector3>(vecId);
            ctx.Write(magId, v.magnitude);
            ctx.Write(sqrId, v.sqrMagnitude);
            return ExecutionResult.Continue(-1);
        };
    }
}