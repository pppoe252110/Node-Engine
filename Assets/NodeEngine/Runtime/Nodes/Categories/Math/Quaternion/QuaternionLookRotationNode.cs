using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Look Rotation")]
public class QuaternionLookRotationNode : BaseNode
{
    [NodePort("Forward", true)] public Vector3 forward;
    [NodePort("Up", true)] public Vector3 upwards;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int fwdId = context.GetInputId("Forward");
        int upId = context.GetInputId("Up");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 fwd = ctx.Read<Vector3>(fwdId);
            Vector3 up = ctx.Read<Vector3>(upId, Vector3.up);
            ctx.Write(outId, Quaternion.LookRotation(fwd, up));
            return ExecutionResult.Continue(-1);
        };
    }
}