using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Random/Point In Circle")]
public class RandomPointInCircleNode : BaseNode
{
    [NodePort("Center", true)] public Vector2 center;
    [NodePort("Radius", true)] public float radius;
    [NodePort("Result", false)] public Vector2 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int centerId = context.GetInputId("Center");
        int radiusId = context.GetInputId("Radius");
        int outId = context.GetOutputId("Result");

        return ctx =>
        {
            Vector2 c = ctx.Read<Vector2>(centerId);
            float r = ctx.Read<float>(radiusId);
            Vector2 randomPoint = c + UnityEngine.Random.insideUnitCircle * r;
            ctx.Write(outId, randomPoint);
            return ExecutionResult.Continue(-1);
        };
    }
}