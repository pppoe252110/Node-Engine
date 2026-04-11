using NodeEngine.Compilation;
using System;
using System.Collections.Generic;

[NodePath("Events/Test")]
public class TestNode : BaseNode
{
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int exitFlow = context.GetFlowId("Out");
        return (ctx) => ExecutionResult.Continue(exitFlow);
    }
}