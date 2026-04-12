using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Distance")]
public class Vector3DistanceNode : BaseNode
{
    [NodePort("A", true)] public Vector3 a;
    [NodePort("B", true)] public Vector3 b;
    [NodePort("Distance", false)] public float distance;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int outId = context.GetOutputId("Distance");
        return ctx => {
            Vector3 va = ctx.Read<Vector3>(aId);
            Vector3 vb = ctx.Read<Vector3>(bId);
            ctx.Write(outId, Vector3.Distance(va, vb));
            return ExecutionResult.Continue(-1);
        };
    }
}