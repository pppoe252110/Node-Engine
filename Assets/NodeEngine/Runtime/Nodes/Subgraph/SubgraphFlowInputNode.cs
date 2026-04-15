using NodeEngine.Compilation;
using System;

[HideInNodeList]
[NodePath("Subgraph/Internal/Flow Input")]
public class SubgraphFlowInputNode : SubgraphInputNodeBase
{
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int outFlow = context.GetFlowId("Out");
        return ctx => ExecutionResult.Continue(outFlow);
    }
}