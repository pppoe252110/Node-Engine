using NodeEngine.Compilation;
using System;

[NodePath("Entity/Set Active")]
public class SetEntityActiveNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Entity", true)] public EntityRef target;
    [NodePort("Active", true)] public bool active;
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int targetId = context.GetInputId("Entity");
        int activeId = context.GetInputId("Active");
        int outFlow = context.GetFlowId("Out");

        return ctx =>
        {
            EntityRef entityRef = ctx.Read<EntityRef>(targetId);
            bool setActive = ctx.Read<bool>(activeId);
            NodeEntity entity = entityRef.Resolve();
            if (entity != null) entity.gameObject.SetActive(setActive);
            return ExecutionResult.Continue(outFlow);
        };
    }
}