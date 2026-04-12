using NodeEngine.Compilation;
using NodeEngine.Input;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/On Mouse Button")]
public class OnMouseButtonNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Button", true)] public MouseButton button;
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int btnId = context.GetInputId("Button");
        int outFlow = context.GetFlowId("Out");

        return ctx =>
        {
            MouseButton b = ctx.Read<MouseButton>(btnId, MouseButton.Left);
            Mouse mouse = Mouse.current;
            if (mouse == null) return ExecutionResult.Stop();

            bool held = b switch
            {
                MouseButton.Left => mouse.leftButton.isPressed,
                MouseButton.Right => mouse.rightButton.isPressed,
                MouseButton.Middle => mouse.middleButton.isPressed,
                MouseButton.Forward => mouse.forwardButton.isPressed,
                MouseButton.Back => mouse.backButton.isPressed,
                _ => false
            };
            return held ? ExecutionResult.Continue(outFlow) : ExecutionResult.Stop();
        };
    }
}