using System;
using UnityEngine;

[NodePath("Variables/Type")]
public class TypeVariableNode : VariableNode
{
    public override VariableType VariableType => VariableType.Type;
    public Type SelectedType => GetValue() as Type;

    public void ChangeSelectedType(Type newType)
    {
        Debug.Log($"TypeVariableNode.ChangeSelectedType: {newType?.Name}");

        SetValue(newType);
    }
}