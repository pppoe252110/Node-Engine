using UnityEngine;

public abstract class VariableUIElement : MonoBehaviour
{
    protected VariableNode _node;
    protected VariableType _type;

    public virtual void Initialize(VariableNode node, VariableType type)
    {
        _node = node;
        _type = type;
    }

    public abstract object GetValue();
    public virtual void SetValue(object value) { }
}