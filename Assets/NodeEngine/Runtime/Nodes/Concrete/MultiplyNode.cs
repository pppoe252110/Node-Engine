using System;

[NodePath("Math/Multiply")]
public class MultiplyNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("Result", false)] public float result;

    public override Func<GraphContext, int> Compile()
    {
        int aId = GetInputId("A");
        int bId = GetInputId("B");
        int resId = GetOutputId("Result");

        return (ctx) =>
        {
            float aVal = Read<float>(ctx, aId);
            float bVal = Read<float>(ctx, bId);
            Write(ctx, resId, aVal * bVal);
            return -1;
        };
    }
}