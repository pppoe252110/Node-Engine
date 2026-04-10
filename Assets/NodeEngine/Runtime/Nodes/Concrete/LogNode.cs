using System;
using System.Collections.Generic;
using UnityEngine;

[NodePath("Debug/Log")]
public class LogNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Message", true)] public object message;
    [NodePort("Out", false, true)] public void Exit() { }

    [NonSerialized] public int NextExitIndex = -1;

    public override void AssignFlowIndices(Dictionary<string, int> flowTargets)
    {
        NextExitIndex = flowTargets.TryGetValue("Out", out var idx) ? idx : -1;
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int msgId = GetInputId("Message");
        int exitFlow = NextExitIndex;

        return (ctx) => {
            object msg = ctx.Memory[msgId];
            Debug.Log($"[NodeLog] {msg ?? "null"}");
            return ExecutionResult.Continue(exitFlow);
        };
    }
}