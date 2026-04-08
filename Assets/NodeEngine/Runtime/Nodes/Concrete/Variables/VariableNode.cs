using System;
using UnityEngine;

public abstract class VariableNode : BaseNode
{
    [NodePort("Value", false)] public object value;
    public NodeValue CachedValue = new NodeValue();
    public MonoBehaviour UIElement { get; set; }
    public abstract VariableType VariableType { get; }

    public VariableNode() : base()
    {
        var port = Ports.Find(p => p.Name == "Value");
        if (port != null)
            port.ValueType = GetSystemType(VariableType);
    }

    public static Type GetSystemType(VariableType type)
    {
        return type switch
        {
            VariableType.Single => typeof(float),
            VariableType.Int => typeof(int),
            VariableType.String => typeof(string),
            VariableType.Bool => typeof(bool),
            VariableType.Vector2 => typeof(Vector2),
            VariableType.Vector3 => typeof(Vector3),
            VariableType.Color => typeof(Color),
            VariableType.Type => typeof(Type),
            _ => typeof(object)
        };
    }

    public override Func<GraphContext, int> Compile()
    {
        int outId = GetOutputId("Value");
        var storage = CachedValue;
        return (ctx) =>
        {
            Write(ctx, outId, storage.GetInnerValue());
            return -1;
        };
    }

    public virtual void SyncCachedValueWithUI()
    {
        if (UIElement is VariableUIElement uiElement)
        {
            object uiValue = uiElement.GetValue();
            if (uiValue != null) CachedValue.SetInnerValue(uiValue);
        }
    }

    public virtual void SyncUIWithCachedValue()
    {
        if (UIElement is VariableUIElement uiElement)
        {
            object cached = CachedValue.GetInnerValue();
            if (cached != null) uiElement.SetValue(cached);
        }
    }

    public object GetValue() => CachedValue.GetInnerValue();
    public void SetValue(object newValue) => CachedValue.SetInnerValue(newValue);
}