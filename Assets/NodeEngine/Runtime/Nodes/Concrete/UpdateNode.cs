using NodeEngine.Compilation;
using System;
using System.Collections.Generic;

[NodePath("Events/Update")]
public class UpdateNode : BaseNode
{
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int exitFlow = context.GetFlowId("Out");
        return (ctx) => ExecutionResult.Continue(exitFlow);
    }
}