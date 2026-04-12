using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Quaternion/Inverse")]
public class InverseQuaternionNode : BaseNode
{
    [NodePort("Quat", true)] public Quaternion quaternion;
    [NodePort("Result", false)] public Quaternion result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int qId = context.GetInputId("Quat");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Quaternion q = ctx.Read<Quaternion>(qId);
            ctx.Write(outId, Quaternion.Inverse(q));
            return ExecutionResult.Continue(-1);
        };
    }
}