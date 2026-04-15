using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Transform/Add Position")]
public class EntityAddPositionNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }

    [NodePort("Target", true)] public EntityRef target;
    [NodePort("X", true)] public float x;
    [NodePort("Y", true)] public float y;
    [NodePort("Z", true)] public float z;

    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int targetId = context.GetInputId("Target");
        int xId = context.GetInputId("X");
        int yId = context.GetInputId("Y");
        int zId = context.GetInputId("Z");
        int outFlow = context.GetFlowId("Out");

        return (ctx) =>
        {
            EntityRef entityRef = ctx.Read<EntityRef>(targetId);
            float deltaX = ctx.Read(xId, 0f);
            float deltaY = ctx.Read(yId, 0f);
            float deltaZ = ctx.Read(zId, 0f);

            NodeEntity entity = entityRef.Resolve();
            if (entity != null)
            {
                Vector3 currentPos = entity.transform.position;
                entity.transform.position = currentPos + new Vector3(deltaX, deltaY, deltaZ);
            }

            return ExecutionResult.Continue(outFlow);
        };
    }
}