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

    public override Func<GraphContext, int> Compile()
    {
        // 1. COMPILATION: Resolve indices ONCE. Zero cost at runtime.
        int nameId = GetInputId("Name");
        int valId = GetInputId("Value");
        int exitFlow = GetFlowId("Exit");

        // 2. EXECUTION: Pure integer array access.
        return (ctx) =>
        {
            string name = Read<string>(ctx, nameId);
            object val = ctx.Memory[valId]; // Direct array access

            if (!string.IsNullOrEmpty(name))
            {
                _globalVariables[name] = val;
            }

            return exitFlow; // Returning a captured int!
        };
    }

    public static object GetGlobalValue(string name) =>
        _globalVariables.TryGetValue(name, out var val) ? val : null;
}