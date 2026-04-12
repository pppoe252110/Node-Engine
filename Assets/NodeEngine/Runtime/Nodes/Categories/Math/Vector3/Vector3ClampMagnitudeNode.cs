using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Math/Vector3/Clamp Magnitude")]
public class Vector3ClampMagnitudeNode : BaseNode
{
    [NodePort("Vector", true)] public Vector3 vector;
    [NodePort("Max Length", true)] public float maxLength;
    [NodePort("Result", false)] public Vector3 result;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int vecId = context.GetInputId("Vector");
        int maxId = context.GetInputId("Max Length");
        int outId = context.GetOutputId("Result");
        return ctx => {
            Vector3 v = ctx.Read<Vector3>(vecId);
            float max = ctx.Read<float>(maxId);
            ctx.Write(outId, Vector3.ClampMagnitude(v, max));
            return ExecutionResult.Continue(-1);
        };
    }
}