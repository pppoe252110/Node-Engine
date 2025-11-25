using System;

[AttributeUsage(AttributeTargets.Method)]
public class NodeValueAttribute : Attribute
{
    public string attributeName;
    public Type type;

    public NodeValueAttribute(string attributeName, Type type)
    {
        this.attributeName = attributeName;
        this.type = type;
    }
}