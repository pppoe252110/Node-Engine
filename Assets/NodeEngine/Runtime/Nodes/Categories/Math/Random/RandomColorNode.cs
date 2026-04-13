using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Random/Color")]
public class RandomColorNode : BaseNode
{
    [NodePort("Result", false)] public Color result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int outId = context.GetOutputId("Result");

        return ctx =>
        {                        
            ctx.Write(outId, UnityEngine.Random.ColorHSV());
            return ExecutionResult.Continue(-1);
        };
    }
}