using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Ping Pong")]
public class PingPongNode : BaseNode
{
    [NodePort("T", true)] public float t;
    [NodePort("Length", true)] public float length;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int tId = context.GetInputId("T");
        int lenId = context.GetInputId("Length");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float tv = ctx.Read<float>(tId);
            float len = ctx.Read<float>(lenId);
            ctx.Write(outId, Mathf.PingPong(tv, len));
            return ExecutionResult.Continue(-1);
        };
    }
}