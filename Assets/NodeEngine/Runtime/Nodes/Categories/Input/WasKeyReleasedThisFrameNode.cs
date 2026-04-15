using NodeEngine.Compilation;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/Was Key Released This Frame")]
public class WasKeyReleasedThisFrameNode : BaseNode
{
    [NodePort("Key", true)] public Key key;
    [NodePort("Released", false)] public bool released;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int keyId = context.GetInputId("Key");
        int outId = context.GetOutputId("Released");

        return ctx =>
        {
            Key k = ctx.Read(keyId, Key.None);
            bool wasReleased = k != Key.None && Keyboard.current != null && Keyboard.current[k].wasReleasedThisFrame;
            ctx.Write(outId, wasReleased);
            return ExecutionResult.Continue(-1);
        };
    }
}