using NodeEngine.Compilation;
using System;
using System.Collections.Generic;

[NodePath("Events/Start")]
public class StartNode : BaseNode
{
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int exitFlow = context.GetFlowId("Out");
        return (ctx) => ExecutionResult.Continue(exitFlow);
    }
}