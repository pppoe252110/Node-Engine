using System;

[NodePath("Conversion/ToString")]
public class ToStringNode : BaseNode
{
    [NodePort("Input", true)] public object input;
    [NodePort("Result", false)] public string result;

    public override Func<GraphContext, int> Compile()
    {
        int inId = GetInputId("Input");
        int outId = GetOutputId("Result");

        return (ctx) =>
        {
            object val = ctx.Memory[inId];
            Write(ctx, outId, val?.ToString() ?? "null");
            return -1;
        };
    }
}