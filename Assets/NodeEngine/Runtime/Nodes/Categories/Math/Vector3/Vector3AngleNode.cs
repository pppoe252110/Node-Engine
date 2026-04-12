using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Angle")]
public class Vector3AngleNode : BaseNode
{
    [NodePort("From", true)] public Vector3 from;
    [NodePort("To", true)] public Vector3 to;
    [NodePort("Angle", false)] public float angle;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int fromId = context.GetInputId("From");
        int toId = context.GetInputId("To");
        int outId = context.GetOutputId("Angle");
        return ctx => {
            Vector3 f = ctx.Read<Vector3>(fromId);
            Vector3 t = ctx.Read<Vector3>(toId);
            ctx.Write(outId, Vector3.Angle(f, t));
            return ExecutionResult.Continue(-1);
        };
    }
}