using NodeEngine.Compilation;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/Was Key Pressed This Frame")]
public class WasKeyPressedThisFrameNode : BaseNode
{
    [NodePort("Key", true)] public Key key;
    [NodePort("Pressed", false)] public bool pressed;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int keyId = context.GetInputId("Key");
        int outId = context.GetOutputId("Pressed");

        return ctx =>
        {
            Key k = ctx.Read(keyId, Key.None);
            bool wasPressed = k != Key.None && Keyboard.current != null && Keyboard.current[k].wasPressedThisFrame;
            ctx.Write(outId, wasPressed);
            return ExecutionResult.Continue(-1);
        };
    }
}