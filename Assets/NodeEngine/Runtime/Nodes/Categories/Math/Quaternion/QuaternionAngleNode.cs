using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Angle")]
public class QuaternionAngleNode : BaseNode
{
    [NodePort("Quat", true)] public Quaternion quaternion;
    [NodePort("Angle", false)] public float angle;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int qId = context.GetInputId("Quat");
        int outId = context.GetOutputId("Angle");
        return ctx => {
            Quaternion q = ctx.Read<Quaternion>(qId);
            float angle;
            Vector3 axis;
            q.ToAngleAxis(out angle, out axis);
            ctx.Write(outId, angle);
            return ExecutionResult.Continue(-1);
        };
    }
}