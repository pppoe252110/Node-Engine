using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/To Euler")]
public class QuaternionToEulerNode : BaseNode
{
    [NodePort("Quat", true)] public Quaternion quaternion;
    [NodePort("Result", false)] public Vector3 euler;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int quatId = context.GetInputId("Quat");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Quaternion q = ctx.Read<Quaternion>(quatId);
            ctx.Write(outId, q.eulerAngles);
            return ExecutionResult.Continue(-1);
        };
    }
}