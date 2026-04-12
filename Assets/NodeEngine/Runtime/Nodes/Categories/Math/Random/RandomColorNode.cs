using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Random/Color")]
public class RandomColorNode : BaseNode
{
    [NodePort("Use HSV", true)] public bool useHsv;
    [NodePort("Result", false)] public Color result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int useHsvId = context.GetInputId("Use HSV");
        int outId = context.GetOutputId("Result");

        return ctx =>
        {
            bool hsv = ctx.Read<bool>(useHsvId);
                        
            ctx.Write(outId, UnityEngine.Random.ColorHSV());
            return ExecutionResult.Continue(-1);
        };
    }
}