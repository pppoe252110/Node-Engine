using System;

[NodePath("Conversion/Convert")]
public class ConvertNode : BaseNode
{
    [NodePort("Input", true)] public object input;
    [NodePort("TargetType", true)] public Type targetType;
    [NodePort("Result", false)] public object result;

    public ConvertNode()
    {
        BindDynamicType("TargetType", "Result");
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int inId = GetInputId("Input");
        int typeId = GetInputId("TargetType");
        int outId = GetOutputId("Result");

        return (ctx) =>
        {
            object val = ctx.Memory[inId];
            Type t = ctx.Memory[typeId] as Type;

            if (val != null && t != null)
            {
                try { ctx.Memory[outId] = Convert.ChangeType(val, t); }
                catch { ctx.Memory[outId] = null; }
            }
            else
            {
                ctx.Memory[outId] = null;
            }

            return ExecutionResult.Continue(-1);
        };
    }
}