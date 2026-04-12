using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
public class NodePortAttribute : Attribute
{
    public string Name;
    public bool IsInput;
    public bool IsFlow;
    public int Order; // Lower numbers appear first (top)

    public NodePortAttribute(string name, bool isInput, bool isFlow = false, int order = 0)
    {
        Name = name;
        IsInput = isInput;
        IsFlow = isFlow;
        Order = order;
    }
}