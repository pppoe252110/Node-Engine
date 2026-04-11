using System;
using NodeEngine.Compilation;

[NodePath("Flow/ForLoop")]
public class ForLoopNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Count", true)] public int count;
    [NodePort("Loop", false, true)] public void LoopBody() { }
    [NodePort("Done", false, true)] public void Completed() { }
    [NodePort("Index", false)] public int currentIndex;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int countId = context.GetInputId("Count");
        int indexId = context.GetOutputId("Index");
        int loopFlow = context.GetFlowId("Loop");
        int doneFlow = context.GetFlowId("Done");
        const int MaxIterations = 10000;

        return (ctx) =>
        {
            int max = Read<int>(ctx, countId);
            max = Math.Min(max, MaxIterations); // safety clamp
            for (int i = 0; i < max; i++)
            {
                Write(ctx, indexId, i);
                if (loopFlow >= 0)
                    ctx.ExecuteFlow(loopFlow);
            }
            return ExecutionResult.Continue(doneFlow);
        };
    }
}