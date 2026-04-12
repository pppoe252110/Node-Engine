using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Engine/Time")]
public class PiNode : BaseNode
{
    [NodePort("PI", false)] public float deltaTime;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int piId = context.GetOutputId("PI");

        return (ctx) => {
            ctx.Write(piId, Mathf.PI);
            return ExecutionResult.Continue(-1);
        };
    }
}