using System;
using UnityEngine;

[NodePath("Transform/Move")]
public class MoveGameObjectNode : BaseNode
{
    [NodePort("In", true, true)] public void In() { }
    [NodePort("Target", true)] public GameObject target;
    [NodePort("Position", true)] public Vector3 position;

    public override Func<GraphContext, int> Compile()
    {
        int targetId = GetInputId("Target");
        int posId = GetInputId("Position");

        return (ctx) =>
        {
            GameObject go = ctx.Memory[targetId] as GameObject;
            Vector3 pos = (Vector3)(ctx.Memory[posId] ?? Vector3.zero);
            if (go != null) go.transform.position = pos;
            return -1;
        };
    }
}