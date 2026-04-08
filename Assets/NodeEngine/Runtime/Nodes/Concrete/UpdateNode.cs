using System;
using System.Collections.Generic;

[NodePath("Events/Update")]
public class UpdateNode : BaseNode
{
    [NodePort("Out", false, true)] public void Out() { }
    [NonSerialized] public int NextExitIndex = -1;

    public override void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        NextExitIndex = flowTargets.TryGetValue("Out", out var idx) ? idx : -1;
    }

    public override Func<GraphContext, int> Compile()
    {
        int exitFlow = NextExitIndex;
        return (ctx) => exitFlow;
    }
}