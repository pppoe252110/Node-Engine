using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Multiply")]
public class MultiplyQuaternionNode : BaseNode
{
    [NodePort("A", true)] public Quaternion a;
    [NodePort("B", true)] public Quaternion b;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Quaternion qa = ctx.Read<Quaternion>(aId);
            Quaternion qb = ctx.Read<Quaternion>(bId);
            ctx.Write(outId, qa * qb);
            return ExecutionResult.Continue(-1);
        };
    }
}