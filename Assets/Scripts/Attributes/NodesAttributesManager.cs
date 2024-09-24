using System;
using UnityEngine;

public class NodesAttributesManager
{

}

[AttributeUsage(AttributeTargets.Method)]
public class NodeValueAttribute : Attribute
{
    public string attributeName;
    public Color attributeColor
    {
        get
        {
            var c = System.Drawing.Color.FromKnownColor(knownColor);
            return new Color32(c.R, c.G, c.B, c.A);
        }
    }
    private System.Drawing.KnownColor knownColor;
    public Type type;

    public NodeValueAttribute(string attributeName, Type type, System.Drawing.KnownColor attributeColor)
    {
        this.attributeName = attributeName;
        this.type = type;
        this.knownColor = attributeColor;
    }
}