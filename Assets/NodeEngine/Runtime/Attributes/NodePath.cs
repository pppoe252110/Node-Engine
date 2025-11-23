using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NodePathAttribute : Attribute
{
    public string Path { get; private set; }

    public NodePathAttribute(string path)
    {
        Path = path;
    }
}