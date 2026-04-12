using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Random/Float Range")]
public class RandomRangeFloatNode : BaseNode
{
    [NodePort("Min", true)] public float min;
    [NodePort("Max", true)] public float max;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int minId = context.GetInputId("Min");
        int maxId = context.GetInputId("Max");
        int outId = context.GetOutputId("Result");
        return ctx => {
            float mn = ctx.Read<float>(minId);
            float mx = ctx.Read<float>(maxId);
            ctx.Write(outId, UnityEngine.Random.Range(mn, mx));
            return ExecutionResult.Continue(-1);
        };
    }
}