using NodeEngine.Compilation;
using System;

[NodePath("Math/Add")]
public class AddNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int resultId = context.GetOutputId("Result");

        return (ctx) => {
            float inA = ctx.Read<float>(aId);
            float inB = ctx.Read<float>(bId);
            ctx.Write(resultId, inA + inB);
            return ExecutionResult.Continue(-1);
        };
    }
}