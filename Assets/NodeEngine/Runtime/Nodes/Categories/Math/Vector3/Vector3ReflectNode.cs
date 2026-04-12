using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Reflect")]
public class Vector3ReflectNode : BaseNode
{
    [NodePort("In Direction", true)] public Vector3 inDirection;
    [NodePort("Normal", true)] public Vector3 normal;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int dirId = context.GetInputId("In Direction");
        int normId = context.GetInputId("Normal");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 dir = ctx.Read<Vector3>(dirId);
            Vector3 norm = ctx.Read<Vector3>(normId);
            ctx.Write(outId, Vector3.Reflect(dir, norm));
            return ExecutionResult.Continue(-1);
        };
    }
}