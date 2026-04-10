using System;
using System.Collections.Generic;
using UnityEngine;

[NodePath("Flow/Throttle")]
public class ThrottleNode : BaseNode
{
    [NodePort("In", true, true)] public void Enter() { }
    [NodePort("Interval", true)] public float interval;
    [NodePort("Out", false, true)] public void Exit() { }

    [NonSerialized] public int NextExitIndex = -1;
    private float _lastPassTime = float.MinValue;

    public override void SetFlowTargets(Dictionary<string, int> flowTargets)
    {
        NextExitIndex = flowTargets.TryGetValue("Out", out var idx) ? idx : -1;
    }

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int intervalId = GetInputId("Interval");
        int exitFlow = NextExitIndex;

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