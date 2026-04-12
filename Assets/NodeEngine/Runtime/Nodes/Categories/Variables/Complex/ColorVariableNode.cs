using UnityEngine;

[NodePath("Variables/Color")]
public class ColorVariableNode : VariableNode<Color>
{
    public ColorVariableNode()
    {
        SetValue(Color.white);
    }
}