using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Entity/Get Position")]
public class GetEntityPositionNode : BaseNode
{
    [NodePort("Entity", true)] public EntityRef target;
    [NodePort("Position", false)] public Vector3 position;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int targetId = context.GetInputId("Entity");
        int outId = context.GetOutputId("Position");

        return ctx =>
        {
            EntityRef entityRef = ctx.Read<EntityRef>(targetId);
            NodeEntity entity = entityRef.Resolve();
            ctx.Write(outId, entity != null ? entity.transform.position : Vector3.zero);
            return ExecutionResult.Continue(-1);
        };
    }
}