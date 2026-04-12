using UnityEngine.InputSystem;

[NodePath("Variables/Key")]
public class KeyVariableNode : VariableNode<Key>
{
    public KeyVariableNode()
    {
        SetValue(Key.None);
    }
}