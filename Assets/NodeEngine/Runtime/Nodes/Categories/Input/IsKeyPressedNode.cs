using NodeEngine.Compilation;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/Is Key Pressed")]
public class IsKeyPressedNode : BaseNode
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
            bool isPressed = k != Key.None && Keyboard.current != null && Keyboard.current[k].isPressed;
            ctx.Write(outId, isPressed);
            return ExecutionResult.Continue(-1);
        };
    }
}