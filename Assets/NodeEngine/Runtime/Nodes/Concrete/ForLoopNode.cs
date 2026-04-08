using System;
using System.Collections.Generic;

[NodePath("Flow/ForLoop")]
public class ForLoopNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Count", true)] public int count;
    [NodePort("Loop", false, true)] public void LoopBody() { }
    [NodePort("Done", false, true)] public void Completed() { }
    [NodePort("Index", false)] public int currentIndex;

    [NonSerialized] public int LoopFlowIndex = -1;
    [NonSerialized] public int DoneFlowIndex = -1;

    public override void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        LoopFlowIndex = flowTargets.TryGetValue("Loop", out var loopIdx) ? loopIdx : -1;
        DoneFlowIndex = flowTargets.TryGetValue("Done", out var doneIdx) ? doneIdx : -1;
    }

    public override Func<GraphContext, int> Compile()
    {
        int countId = GetInputId("Count");
        int indexId = GetOutputId("Index");
        int loopFlow = LoopFlowIndex;
        int doneFlow = DoneFlowIndex;

        return (ctx) =>
        {
            int max = Read<int>(ctx, countId);
            for (int i = 0; i < max; i++)
            {
                Write(ctx, indexId, i);
                if (loopFlow >= 0) ctx.ExecuteFlow(loopFlow);
            }
            return doneFlow;
        };
    }
}