using NodeEngine.Compilation;
using NodeEngine.Input;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/Was Mouse Button Released This Frame")]
public class WasMouseButtonReleasedThisFrameNode : BaseNode
{
    [NodePort("Button", true)] public MouseButton button;
    [NodePort("Released", false)] public bool released;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int btnId = context.GetInputId("Button");
        int outId = context.GetOutputId("Released");

        return ctx =>
        {
            MouseButton b = ctx.Read(btnId, MouseButton.Left);
            Mouse mouse = Mouse.current;
            bool wasReleased = mouse != null && b switch
            {
                MouseButton.Left => mouse.leftButton.wasReleasedThisFrame,
                MouseButton.Right => mouse.rightButton.wasReleasedThisFrame,
                MouseButton.Middle => mouse.middleButton.wasReleasedThisFrame,
                MouseButton.Forward => mouse.forwardButton.wasReleasedThisFrame,
                MouseButton.Back => mouse.backButton.wasReleasedThisFrame,
                _ => false
            };
            ctx.Write(outId, wasReleased);
            return ExecutionResult.Continue(-1);
        };
    }
}