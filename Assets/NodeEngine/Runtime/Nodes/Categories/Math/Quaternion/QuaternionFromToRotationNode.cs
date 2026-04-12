using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/From To Rotation")]
public class QuaternionFromToRotationNode : BaseNode
{
    [NodePort("From", true)] public Vector3 fromDirection;
    [NodePort("To", true)] public Vector3 toDirection;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int fromId = context.GetInputId("From");
        int toId = context.GetInputId("To");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 from = ctx.Read<Vector3>(fromId);
            Vector3 to = ctx.Read<Vector3>(toId);
            ctx.Write(outId, Quaternion.FromToRotation(from, to));
            return ExecutionResult.Continue(-1);
        };
    }
}