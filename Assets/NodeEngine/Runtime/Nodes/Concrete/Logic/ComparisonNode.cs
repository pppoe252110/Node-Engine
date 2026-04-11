using NodeEngine.Compilation;
using System;
using UnityEngine;

public enum ComparisonOperation
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

[NodePath("Logic/Comparison")]
public class ComparisonNode : BaseNode
{
    [NodePort("A", true)] public float a;
    [NodePort("B", true)] public float b;
    [NodePort("Operation", true)] public ComparisonOperation operation;
    [NodePort("Result", false)] public bool result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int aId = context.GetInputId("A");
        int bId = context.GetInputId("B");
        int opId = context.GetInputId("Operation");
        int resId = context.GetOutputId("Result");

        return (ctx) =>
        {
            float valA = Read<float>(ctx, aId);
            float valB = Read<float>(ctx, bId);
            ComparisonOperation op = Read<ComparisonOperation>(ctx, opId);

            bool output = op switch
            {
                ComparisonOperation.Equal => Mathf.Approximately(valA, valB),
                ComparisonOperation.NotEqual => !Mathf.Approximately(valA, valB),
                ComparisonOperation.Greater => valA > valB,
                ComparisonOperation.GreaterOrEqual => valA >= valB,
                ComparisonOperation.Less => valA < valB,
                ComparisonOperation.LessOrEqual => valA <= valB,
                _ => false
            };

            Write(ctx, resId, output);
            return ExecutionResult.Continue(-1);
        };
    }
}