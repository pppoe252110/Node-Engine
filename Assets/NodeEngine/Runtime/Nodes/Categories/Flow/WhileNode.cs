using NodeEngine.Compilation;
using System;

[NodePath("Flow/While")]
public class WhileNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Condition", true)] public bool condition;
    [NodePort("Body", false, true)] public void LoopBody() { }
    [NodePort("Out", false, true)] public void Completed() { }


    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int condId = context.GetInputId("Condition");
        int bodyFlow = context.GetFlowId("Body");
        int exitFlow = context.GetFlowId("Out");

        return (ctx) =>
        {
            while (ctx.Read<bool>(condId))
            {
                if (bodyFlow >= 0)
                {
                    ctx.ExecuteSubFlow(bodyFlow);
                }
            }
            return ExecutionResult.Continue(exitFlow);
        };
    }
}