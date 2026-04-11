using System;

[AttributeUsage(AttributeTargets.Class)]
public class NodePathAttribute : Attribute
{
    public string Path { get; }
    public NodePathAttribute(string path) => Path = path;
}
