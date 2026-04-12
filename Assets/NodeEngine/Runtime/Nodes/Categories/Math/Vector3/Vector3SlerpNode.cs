using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Slerp")]
public class Vector3SlerpNode : BaseNode
{
    [NodePort("A", true)] public Vector3 a;
    [NodePort("B", true)] public Vector3 b;
    [NodePort("T", true)] public float t;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int tId = context.GetInputId("T");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 va = ctx.Read<Vector3>(aId);
            Vector3 vb = ctx.Read<Vector3>(bId);
            float tv = ctx.Read<float>(tId);
            ctx.Write(outId, Vector3.Slerp(va, vb, tv));
            return ExecutionResult.Continue(-1);
        };
    }
}