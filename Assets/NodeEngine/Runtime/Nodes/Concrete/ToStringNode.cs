using NodeEngine.Compilation;
using System;

[NodePath("Conversion/ToString")]
public class ToStringNode : BaseNode
{
    [NodePort("Input", true)] public object input;
    [NodePort("Result", false)] public string result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int inId = context.GetInputId("Input");
        int outId = context.GetOutputId("Result");

        return (ctx) => {
            object val = ctx.Memory[inId];
            Write(ctx, outId, val?.ToString() ?? "null");
            return ExecutionResult.Continue(-1);
        };
    }
}