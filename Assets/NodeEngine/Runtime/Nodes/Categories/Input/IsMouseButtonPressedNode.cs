using NodeEngine.Compilation;
using NodeEngine.Input;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/Is Mouse Button Pressed")]
public class IsMouseButtonPressedNode : BaseNode
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
            bool isPressed = mouse != null && b switch
            {
                MouseButton.Left => mouse.leftButton.isPressed,
                MouseButton.Right => mouse.rightButton.isPressed,
                MouseButton.Middle => mouse.middleButton.isPressed,
                MouseButton.Forward => mouse.forwardButton.isPressed,
                MouseButton.Back => mouse.backButton.isPressed,
                _ => false
            };
            ctx.Write(outId, isPressed);
            return ExecutionResult.Continue(-1);
        };
    }
}