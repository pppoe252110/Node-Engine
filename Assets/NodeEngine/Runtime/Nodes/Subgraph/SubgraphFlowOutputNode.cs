using NodeEngine.Compilation;
using System;

public class SubgraphFlowOutputNode : SubgraphOutputNodeBase
{
    [NodePort("In", true, true)] public void In() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        return ctx => ExecutionResult.Stop();
    }
}