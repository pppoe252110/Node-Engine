using System;
using System.Collections.Generic;

[NodePath("Variables/Get Global")]
public class GetGlobalNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Out", false, true)] public void Exit() { }
    [NodePort("Name", true)] public string variableName;
    [NodePort("Type", true)] public Type expectedType;
    [NodePort("Value", false)] public object value;

    [NonSerialized] public int NextExitIndex = -1;

    public GetGlobalNode()
    {
        BindDynamicType("Type", "Value");
    }

    public override void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        NextExitIndex = flowTargets.TryGetValue("Out", out var idx) ? idx : -1;
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int nameId = GetInputId("Name");
        int typeId = GetInputId("Type");
        int outId = GetOutputId("Value");
        int exitFlow = NextExitIndex;

        return (ctx) =>
        {
            string name = Read<string>(ctx, nameId);
            object rawValue = SetGlobalNode.GetGlobalValue(name);
            Type targetType = ctx.Memory[typeId] as Type;

            if (rawValue != null && targetType != null && targetType.IsInstanceOfType(rawValue))
                Write(ctx, outId, rawValue);
            else
                Write(ctx, outId, null);

            return ExecutionResult.Continue(exitFlow);
        };
    }
}