using NodeEngine.Input;

[NodePath("Variables/Mouse Button")]
public class MouseButtonVariableNode : VariableNode<MouseButton>
{
    public MouseButtonVariableNode()
    {
        SetValue(MouseButton.Left);
    }
}