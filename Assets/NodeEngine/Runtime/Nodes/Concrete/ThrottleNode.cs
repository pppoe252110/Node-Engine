using NodeEngine.Compilation;
using System;
using System.Collections.Generic;
using UnityEngine;

[NodePath("Flow/Throttle")]
public class ThrottleNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Interval", true)] public float interval;
    [NodePort("Out", false, true)] public void Exit() { }

    private float _lastPassTime = float.MinValue;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int intervalId = context.GetInputId("Interval");
        int exitFlow = context.GetFlowId("Out");

        return (ctx) =>
        {
            float intervalVal = Read<float>(ctx, intervalId);
            float now = Time.time;

            if (_lastPassTime < 0 || now - _lastPassTime >= intervalVal)
            {
                _lastPassTime = now;
                return ExecutionResult.Continue(exitFlow);
            }

            // Interval not met – stop this branch
            return ExecutionResult.Stop();
        };
    }
}