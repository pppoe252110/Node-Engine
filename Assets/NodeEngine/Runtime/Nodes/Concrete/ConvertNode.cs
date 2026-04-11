using NodeEngine.Compilation;
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

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int inId = context.GetInputId("Input");
        int typeId = context.GetInputId("TargetType");
        int outId = context.GetOutputId("Result");

        return (ctx) =>
        {
            object val = ctx.Read<object>(inId);
            Type t = ctx.Read<Type>(typeId);

            if (val != null && t != null)
            {
                try
                {
                    object converted = Convert.ChangeType(val, t);
                    ctx.Write(outId, converted);
                }
                catch
                {
                    ctx.Write(outId, null);
                }
            }
            else
            {
                ctx.Write(outId, null);
            }

            return ExecutionResult.Continue(-1);
        };
    }
}