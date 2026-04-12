using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Move Towards")]
public class Vector3MoveTowardsNode : BaseNode
{
    [NodePort("Current", true)] public Vector3 current;
    [NodePort("Target", true)] public Vector3 target;
    [NodePort("Max Delta", true)] public float maxDelta;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int curId = context.GetInputId("Current");
        int tgtId = context.GetInputId("Target");
        int deltaId = context.GetInputId("Max Delta");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 cur = ctx.Read<Vector3>(curId);
            Vector3 tgt = ctx.Read<Vector3>(tgtId);
            float delta = ctx.Read<float>(deltaId);
            ctx.Write(outId, Vector3.MoveTowards(cur, tgt, delta));
            return ExecutionResult.Continue(-1);
        };
    }
}