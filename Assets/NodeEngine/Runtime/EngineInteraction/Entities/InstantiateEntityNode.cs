using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Entity/Instantiate")]
public class InstantiateEntityNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Prefab", true)] public GameObject prefab;
    [NodePort("Position", true)] public Vector3 position;
    [NodePort("Rotation", true)] public Quaternion rotation;
    [NodePort("Out Entity", false)] public EntityRef spawnedEntity;
    [NodePort("Out", false, true)] public void Out() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int prefabId = context.GetInputId("Prefab");
        int posId = context.GetInputId("Position");
        int rotId = context.GetInputId("Rotation");
        int outEntityId = context.GetOutputId("Out Entity");
        int outFlow = context.GetFlowId("Out");

        return ctx =>
        {
            GameObject prefab = ctx.Read<GameObject>(prefabId);
            Vector3 pos = ctx.Read<Vector3>(posId);
            Quaternion rot = ctx.Read<Quaternion>(rotId, Quaternion.identity);

            if (prefab != null)
            {
                GameObject instance = UnityEngine.Object.Instantiate(prefab, pos, rot);
                NodeEntity nodeEntity = instance.GetComponent<NodeEntity>();
                if (nodeEntity == null) nodeEntity = instance.AddComponent<NodeEntity>();
                ctx.Write(outEntityId, new EntityRef(nodeEntity.Id));
            }
            else
            {
                ctx.Write(outEntityId, new EntityRef());
            }
            return ExecutionResult.Continue(outFlow);
        };
    }
}