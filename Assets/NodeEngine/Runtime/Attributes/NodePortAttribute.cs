using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
public class NodePortAttribute : Attribute
{
    public string Name;
    public bool IsInput;
    public bool IsFlow; // True = Execution (Action), False = Data

    public NodePortAttribute(string name, bool isInput, bool isFlow = false)
    {
        Name = name;
        IsInput = isInput;
        IsFlow = isFlow;
    }
}