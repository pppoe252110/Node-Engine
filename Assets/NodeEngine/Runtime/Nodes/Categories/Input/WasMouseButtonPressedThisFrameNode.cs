using NodeEngine.Compilation;
using NodeEngine.Input;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/Was Mouse Button Pressed This Frame")]
public class WasMouseButtonPressedThisFrameNode : BaseNode
{
    [NodePort("Button", true)] public MouseButton button;
    [NodePort("Pressed", false)] public bool pressed;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int btnId = context.GetInputId("Button");
        int outId = context.GetOutputId("Pressed");

        return ctx =>
        {
            MouseButton b = ctx.Read(btnId, MouseButton.Left);
            Mouse mouse = Mouse.current;
            bool wasPressed = mouse != null && b switch
            {
                MouseButton.Left => mouse.leftButton.wasPressedThisFrame,
                MouseButton.Right => mouse.rightButton.wasPressedThisFrame,
                MouseButton.Middle => mouse.middleButton.wasPressedThisFrame,
                MouseButton.Forward => mouse.forwardButton.wasPressedThisFrame,
                MouseButton.Back => mouse.backButton.wasPressedThisFrame,
                _ => false
            };
            ctx.Write(outId, wasPressed);
            return ExecutionResult.Continue(-1);
        };
    }
}