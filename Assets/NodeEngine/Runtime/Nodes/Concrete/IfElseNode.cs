using NodeEngine.Compilation;
using System;
using System.Collections.Generic;

[NodePath("Flow/IfElse")]
public class IfElseNode : BaseNode
{
    [NodePort("Condition", true)] public bool condition;
    [NodePort("True", false, true)] public void TrueBranch() { }
    [NodePort("False", false, true)] public void FalseBranch() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int condId = context.GetInputId("Condition");
        int trueIdx = context.GetFlowId("True");
        int falseIdx = context.GetFlowId("False");

        return (ctx) => {
            bool cond = Read<bool>(ctx, condId);
            return ExecutionResult.Continue(cond ? trueIdx : falseIdx);
        };
    }
}