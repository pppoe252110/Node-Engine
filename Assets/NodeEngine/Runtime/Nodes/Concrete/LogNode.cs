using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Debug/Log")]
public class LogNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Message", true)] public object message;
    [NodePort("Out", false, true)] public void Exit() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int msgId = context.GetInputId("Message");
        int exitFlow = context.GetFlowId("Out");

        return (ctx) => {
            object msg = ctx.Memory[msgId];
            Debug.Log($"[NodeLog] {msg ?? "null"}");
            return ExecutionResult.Continue(exitFlow);
        };
    }
}