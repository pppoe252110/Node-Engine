using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Rotate Towards")]
public class RotateTowardsNode : BaseNode
{
    [NodePort("From", true)] public Quaternion from;
    [NodePort("To", true)] public Quaternion to;
    [NodePort("Max Delta", true)] public float maxDegreesDelta;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int fromId = context.GetInputId("From");
        int toId = context.GetInputId("To");
        int deltaId = context.GetInputId("Max Delta");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Quaternion f = ctx.Read<Quaternion>(fromId);
            Quaternion t = ctx.Read<Quaternion>(toId);
            float delta = ctx.Read<float>(deltaId);
            ctx.Write(outId, Quaternion.RotateTowards(f, t, delta));
            return ExecutionResult.Continue(-1);
        };
    }
}