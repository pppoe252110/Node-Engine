using NodeEngine.Compilation;
using System;

[NodePath("Math/Random/Int Range")]
public class RandomRangeIntNode : BaseNode
{
    [NodePort("Min", true)] public int min;
    [NodePort("Max", true)] public int max;
    [NodePort("Result", false)] public int result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int minId = context.GetInputId("Min");
        int maxId = context.GetInputId("Max");
        int outId = context.GetOutputId("Result");
        return ctx => {
            int mn = ctx.Read<int>(minId);
            int mx = ctx.Read<int>(maxId);

            ctx.Write(outId, UnityEngine.Random.Range(mn, mx));
            return ExecutionResult.Continue(-1);
        };
    }
}