using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Project")]
public class Vector3ProjectNode : BaseNode
{
    [NodePort("Vector", true)] public Vector3 vector;
    [NodePort("On Normal", true)] public Vector3 onNormal;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int vecId = context.GetInputId("Vector");
        int normId = context.GetInputId("On Normal");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 v = ctx.Read<Vector3>(vecId);
            Vector3 n = ctx.Read<Vector3>(normId);
            ctx.Write(outId, Vector3.Project(v, n));
            return ExecutionResult.Continue(-1);
        };
    }
}