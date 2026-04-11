using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Transform/Move")]
public class MoveGameObjectNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Target", true)] public GameObject target;
    [NodePort("Position", true)] public Vector3 position;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int targetId = context.GetInputId("Target");
        int posId = context.GetInputId("Position");

        return (ctx) => {
            GameObject go = ctx.Read<GameObject>(targetId);
            Vector3 pos = ctx.Read<Vector3>(posId);
            if (go != null) go.transform.position = pos;
            return ExecutionResult.Continue(-1);
        };
    }
}