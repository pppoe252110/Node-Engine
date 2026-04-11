using NodeEngine.Compilation;
using System;
using System.Collections.Generic;

[NodePath("Variables/Set Global")]
public class SetGlobalNode : BaseNode
{
    private static Dictionary<string, object> _globalVariables = new();

    [NodePort("Enter", true, true)] public void Enter() { }
    [NodePort("Name", true)] public string variableName;
    [NodePort("Value", true)] public object value;
    [NodePort("Exit", false, true)] public void Exit() { }

    public SetGlobalNode() : base()
    {
        BindDynamicType("Value", "Value");
    }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int nameId = context.GetInputId("Name");
        int valId = context.GetInputId("Value");
        int exitFlow = context.GetFlowId("Exit");

        return (ctx) => {
            string name = Read<string>(ctx, nameId);
            object val = ctx.Memory[valId];
            if (!string.IsNullOrEmpty(name))
                _globalVariables[name] = val;
            return ExecutionResult.Continue(exitFlow);
        };
    }

    public static object GetGlobalValue(string name) =>
        _globalVariables.TryGetValue(name, out var val) ? val : null;
}