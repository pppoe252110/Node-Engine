using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Transform/Move")]
public class MoveEntityNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }

    [NodePort("Target", true)] public EntityRef target;
    [NodePort("Position", true)] public Vector3 position;
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int targetId = context.GetInputId("Target");
        int posId = context.GetInputId("Position");
        int outFlow = context.GetFlowId("Out");

        return (ctx) => {
            EntityRef entityRef = ctx.Read<EntityRef>(targetId);
            Vector3 pos = ctx.Read<Vector3>(posId);

            NodeEntity entity = entityRef.Resolve();

            if (entity != null)
            {
                entity.transform.position = pos;
            }

            return ExecutionResult.Continue(outFlow);
        };
    }
}