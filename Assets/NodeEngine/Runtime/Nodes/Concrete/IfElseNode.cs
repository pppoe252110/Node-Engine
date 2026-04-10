using System;
using System.Collections.Generic;

[NodePath("Flow/IfElse")]
public class IfElseNode : BaseNode
{
    [NodePort("Condition", true)] public bool condition;
    [NodePort("True", false, true)] public void TrueBranch() { }
    [NodePort("False", false, true)] public void FalseBranch() { }

    [NonSerialized] public int TrueExitIndex = -1;
    [NonSerialized] public int FalseExitIndex = -1;

    public override void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        TrueExitIndex = flowTargets.TryGetValue("True", out var tIdx) ? tIdx : -1;
        FalseExitIndex = flowTargets.TryGetValue("False", out var fIdx) ? fIdx : -1;
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int condId = GetInputId("Condition");
        int trueIdx = TrueExitIndex;
        int falseIdx = FalseExitIndex;

        return (ctx) => {
            bool cond = Read<bool>(ctx, condId);
            return ExecutionResult.Continue(cond ? trueIdx : falseIdx);
        };
    }
}