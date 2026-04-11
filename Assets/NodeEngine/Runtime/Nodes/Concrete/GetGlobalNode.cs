using NodeEngine.Compilation;
using System;

[NodePath("Variables/Get Global")]
public class GetGlobalNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Out", false, true)] public void Exit() { }
    [NodePort("Name", true)] public string variableName;
    [NodePort("Type", true)] public Type expectedType;
    [NodePort("Value", false)] public object value;

    public GetGlobalNode()
    {
        BindDynamicType("Type", "Value");
    }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int nameId = context.GetInputId("Name");
        int typeId = context.GetInputId("Type");
        int outId = context.GetOutputId("Value");
        int exitFlow = context.GetFlowId("Out");

        return (ctx) =>
        {
            string name = ctx.Read<string>(nameId);
            object rawValue = SetGlobalNode.GetGlobalValue(name);
            Type targetType = ctx.Read<Type>(typeId);

            if (rawValue != null && targetType != null && targetType.IsInstanceOfType(rawValue))
                ctx.Write(outId, rawValue);
            else
                ctx.Write(outId, null);

            return ExecutionResult.Continue(exitFlow);
        };
    }
}