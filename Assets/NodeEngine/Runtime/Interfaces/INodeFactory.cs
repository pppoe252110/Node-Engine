using System;

public interface INodeFactory
{
    public BaseNode CreateNode(Type nodeType);
}