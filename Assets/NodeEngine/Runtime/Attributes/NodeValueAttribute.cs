using System;

[AttributeUsage(AttributeTargets.Method)]
public class NodeValueAttribute : Attribute
{
    public string Name { get; }
    public Type type { get; }
    public NodeValueAttribute(string name, Type type)
    {
        Name = name;
        this.type = type;
    }
}
