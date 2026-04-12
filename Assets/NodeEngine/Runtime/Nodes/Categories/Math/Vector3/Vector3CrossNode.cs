using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Cross")]
public class Vector3CrossNode : BaseNode
{
    [NodePort("A", true)] public Vector3 a;
    [NodePort("B", true)] public Vector3 b;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 va = ctx.Read<Vector3>(aId);
            Vector3 vb = ctx.Read<Vector3>(bId);
            ctx.Write(outId, Vector3.Cross(va, vb));
            return ExecutionResult.Continue(-1);
        };
    }
}