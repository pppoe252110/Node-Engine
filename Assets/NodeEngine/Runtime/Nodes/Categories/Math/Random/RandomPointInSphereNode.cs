using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Random/Point In Sphere")]
public class RandomPointInSphereNode : BaseNode
{
    [NodePort("Center", true)] public Vector3 center;
    [NodePort("Radius", true)] public float radius;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int centerId = context.GetInputId("Center");
        int radiusId = context.GetInputId("Radius");
        int outId = context.GetOutputId("Result");

        return ctx =>
        {
            Vector3 c = ctx.Read<Vector3>(centerId);
            float r = ctx.Read<float>(radiusId);
            Vector3 randomPoint = c + UnityEngine.Random.insideUnitSphere * r;
            ctx.Write(outId, randomPoint);
            return ExecutionResult.Continue(-1);
        };
    }
}