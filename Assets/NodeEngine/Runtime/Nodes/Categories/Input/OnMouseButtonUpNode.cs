using NodeEngine.Compilation;
using NodeEngine.Input;
using System;
using UnityEngine.InputSystem;

[NodePath("Input/On Mouse Button Up")]
public class OnMouseButtonUpNode : BaseNode
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

            bool released = b switch
            {
                MouseButton.Left => mouse.leftButton.wasReleasedThisFrame,
                MouseButton.Right => mouse.rightButton.wasReleasedThisFrame,
                MouseButton.Middle => mouse.middleButton.wasReleasedThisFrame,
                MouseButton.Forward => mouse.forwardButton.wasReleasedThisFrame,
                MouseButton.Back => mouse.backButton.wasReleasedThisFrame,
                _ => false
            };
            return released ? ExecutionResult.Continue(outFlow) : ExecutionResult.Stop();
        };
    }
}