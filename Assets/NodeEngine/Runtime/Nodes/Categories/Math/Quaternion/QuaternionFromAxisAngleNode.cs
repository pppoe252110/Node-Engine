using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Axis Angle")]
public class QuaternionFromAxisAngleNode : BaseNode
{
    [NodePort("Axis", true)] public Vector3 axis;
    [NodePort("Angle", true)] public float angle;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int axisId = context.GetInputId("Axis");
        int angleId = context.GetInputId("Angle");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 ax = ctx.Read<Vector3>(axisId);
            float ang = ctx.Read<float>(angleId);
            ctx.Write(outId, Quaternion.AngleAxis(ang, ax));
            return ExecutionResult.Continue(-1);
        };
    }
}