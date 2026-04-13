using NodeEngine.Compilation;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/On Key")]
public class OnKeyNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Key", true)] public Key key;
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int keyId = context.GetInputId("Key");
        int outFlow = context.GetFlowId("Out");

        return ctx =>
        {
            Key k = ctx.Read(keyId, Key.None);

            if (k == Key.None)
                return ExecutionResult.Stop();

            if (Keyboard.current == null) 
                return ExecutionResult.Stop();

            if (Keyboard.current[k].isPressed)
                return ExecutionResult.Continue(outFlow);

            return ExecutionResult.Stop();
        };
    }
}