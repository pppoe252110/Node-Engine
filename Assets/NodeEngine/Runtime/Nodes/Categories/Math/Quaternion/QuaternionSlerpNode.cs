using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Slerp")]
public class QuaternionSlerpNode : BaseNode
{
    [NodePort("From", true)] public Quaternion from;
    [NodePort("To", true)] public Quaternion to;
    [NodePort("T", true)] public float t;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int fromId = context.GetInputId("From");
        int toId = context.GetInputId("To");
        int tId = context.GetInputId("T");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Quaternion a = ctx.Read<Quaternion>(fromId);
            Quaternion b = ctx.Read<Quaternion>(toId);
            float tVal = ctx.Read<float>(tId);
            ctx.Write(outId, Quaternion.Slerp(a, b, tVal));
            return ExecutionResult.Continue(-1);
        };
    }
}