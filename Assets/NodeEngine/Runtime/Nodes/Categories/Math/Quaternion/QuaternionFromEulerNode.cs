using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/From Euler")]
public class QuaternionFromEulerNode : BaseNode
{
    [NodePort("Euler", true)] public Vector3 euler;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int eulerId = context.GetInputId("Euler");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 e = ctx.Read<Vector3>(eulerId);
            ctx.Write(outId, Quaternion.Euler(e));
            return ExecutionResult.Continue(-1);
        };
    }
}