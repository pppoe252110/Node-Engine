using System;
using UnityEngine;

public abstract class VariableUIElement : MonoBehaviour
{
    protected IVariableNode TargetNode;

    // The registry will ask the UI element if it supports a given Type
    public abstract bool CanBind(Type valueType);

    public virtual void Bind(IVariableNode node)
    {
        TargetNode = node;
        TargetNode.OnUntypedValueChanged += OnNodeValueChanged;

        // Initialize UI with current value
        OnNodeValueChanged(TargetNode.GetUntypedValue());
    }

    protected virtual void OnDestroy()
    {
        if (TargetNode != null)
        {
            TargetNode.OnUntypedValueChanged -= OnNodeValueChanged;
        }
    }

    // Called when the underlying Node logic changes the value
    protected abstract void OnNodeValueChanged(object newValue);
}