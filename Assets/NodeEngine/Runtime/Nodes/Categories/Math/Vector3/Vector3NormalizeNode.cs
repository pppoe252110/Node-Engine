using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Normalize")]
public class Vector3NormalizeNode : BaseNode
{
    [NodePort("Vector", true)] public Vector3 vector;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int vecId = context.GetInputId("Vector");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 v = ctx.Read<Vector3>(vecId);
            ctx.Write(outId, v.normalized);
            return ExecutionResult.Continue(-1);
        };
    }
}