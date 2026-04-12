using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Dot")]
public class Vector3DotNode : BaseNode
{
    [NodePort("A", true)] public Vector3 a;
    [NodePort("B", true)] public Vector3 b;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 va = ctx.Read<Vector3>(aId);
            Vector3 vb = ctx.Read<Vector3>(bId);
            ctx.Write(outId, Vector3.Dot(va, vb));
            return ExecutionResult.Continue(-1);
        };
    }
}