using NodeEngine.Compilation;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[NodePath("Input/On Key Down")]
public class OnKeyDownNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }  // Add this flow input
    [NodePort("Key", true)] public Key key;
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int keyId = context.GetInputId("Key");
        int outFlow = context.GetFlowId("Out");

        return ctx =>
        {
            Key k = ctx.Read<Key>(keyId, Key.None);

            if (Keyboard.current == null)
            {
                Debug.LogWarning("[OnKeyDownNode] Keyboard.current is null");
                return ExecutionResult.Stop();
            }

            if (Keyboard.current[k].wasPressedThisFrame)
            {
                Debug.Log($"[OnKeyDownNode] Key {k} pressed!");
                return ExecutionResult.Continue(outFlow);
            }

            return ExecutionResult.Stop();
        };
    }
}