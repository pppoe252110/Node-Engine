using System;

[NodePath("Variables/Type")]
public class TypeVariableNode : VariableNode<Type>
{
    public void ChangeSelectedType(Type newType)
    {
        SetValue(newType);
        UpdatePortType("Value", newType);
    }
}