using NodeEngine.Compilation;
using NodeEngine.Input;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/On Mouse Button Down")]
public class OnMouseButtonDownNode : BaseNode
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

            bool pressed = b switch
            {
                MouseButton.Left => mouse.leftButton.wasPressedThisFrame,
                MouseButton.Right => mouse.rightButton.wasPressedThisFrame,
                MouseButton.Middle => mouse.middleButton.wasPressedThisFrame,
                MouseButton.Forward => mouse.forwardButton.wasPressedThisFrame,
                MouseButton.Back => mouse.backButton.wasPressedThisFrame,
                _ => false
            };

            return pressed ? ExecutionResult.Continue(outFlow) : ExecutionResult.Stop();
        };
    }
}