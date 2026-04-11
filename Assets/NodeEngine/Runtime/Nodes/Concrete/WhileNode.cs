using NodeEngine.Compilation;
using System;
using System.Collections.Generic;

[NodePath("Flow/While")]
public class WhileNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Condition", true)] public bool condition;
    [NodePort("Body", false, true)] public void LoopBody() { }
    [NodePort("Exit", false, true)] public void Completed() { }


    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int condId = context.GetInputId("Condition");
        int bodyFlow = context.GetFlowId("Body");
        int exitFlow = context.GetFlowId("Exit");

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