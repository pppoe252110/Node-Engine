using UnityEngine;

public abstract class VariableUIElement : MonoBehaviour
{
    protected VariableNode _node;

    public abstract object GetValue();

    public virtual void Initialize(VariableNode node, VariableType type)
    {
        _node = node;
    }
}