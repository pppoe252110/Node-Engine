using NodeEngine.Compilation;
using System;

[NodePath("Flow/IfElse")]
public class BranchNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Condition", true)] public bool condition;
    [NodePort("True", false, true)] public void TrueBranch() { }
    [NodePort("False", false, true)] public void FalseBranch() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int condId = context.GetInputId("Condition");
        int trueFlow = context.GetFlowId("True");
        int falseFlow = context.GetFlowId("False");

        return (ctx) => {
            bool cond = ctx.Read<bool>(condId);
            return ExecutionResult.Continue(cond ? trueFlow : falseFlow);
        };
    }
}