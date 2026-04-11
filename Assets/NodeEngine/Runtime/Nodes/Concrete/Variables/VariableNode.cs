using System;
using UnityEngine;

public abstract class VariableNode : BaseNode
{
    [NodePort("Value", false)] public object value;
    public NodeValue CachedValue = new NodeValue();
    public abstract VariableType VariableType { get; }
    public event Action<object> OnValueChanged;

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
            VariableType.ComparisonOperation => typeof(ComparisonOperation),
            _ => typeof(object)
        };
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int outId = GetOutputId("Value");
        var storage = CachedValue;
        return (ctx) => {
            Write(ctx, outId, storage.GetInnerValue());
            return ExecutionResult.Continue(-1);
        };
    }

    public object GetValue() => CachedValue.GetInnerValue();
    public void SetValue(object newValue)
    {
        CachedValue.SetInnerValue(newValue);
        OnValueChanged?.Invoke(newValue);
    }
}