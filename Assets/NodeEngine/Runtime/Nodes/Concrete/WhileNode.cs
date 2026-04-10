using System;
using System.Collections.Generic;

[NodePath("Flow/While")]
public class WhileNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Condition", true)] public bool condition;
    [NodePort("Body", false, true)] public void LoopBody() { }
    [NodePort("Exit", false, true)] public void Completed() { }

    [NonSerialized] public int BodyFlowIndex = -1;
    [NonSerialized] public int ExitFlowIndex = -1;

    public override void SetFlowTargets(Dictionary<string, int> flowTargets)
    {
        BodyFlowIndex = flowTargets.TryGetValue("Body", out var bodyIdx) ? bodyIdx : -1;
        ExitFlowIndex = flowTargets.TryGetValue("Exit", out var exitIdx) ? exitIdx : -1;
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int condId = GetInputId("Condition");
        int bodyFlow = BodyFlowIndex;
        int exitFlow = ExitFlowIndex;

        return (ctx) =>
        {
            while (Read<bool>(ctx, condId))
            {
                if (bodyFlow >= 0)
                    ctx.ExecuteFlow(bodyFlow);
            }
            return ExecutionResult.Continue(exitFlow);
        };
    }
}