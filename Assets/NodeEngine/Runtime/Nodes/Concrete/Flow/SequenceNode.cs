using NodeEngine.Compilation;
using System;

[NodePath("Flow/Sequence")]
public class SequenceNode : BaseNode
{
    [NodePort("In", true, true, order: 0)] public void Enter() { }

    [NodePort("First", false, true, order: 1)] public void Out0() { }
    [NodePort("Then", false, true, order: 2)] public void Out1() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext ctx)
    {
        int first = ctx.GetFlowId("First");
        int then = ctx.GetFlowId("Then");

        return context =>
        {
            context.PushFlow(then);
            context.PushFlow(first);

            return ExecutionResult.Stop();
        };
    }
}