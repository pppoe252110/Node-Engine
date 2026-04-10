using System;
using UnityEngine;

[NodePath("Engine/Time")]
public class TimeNode : BaseNode
{
    [NodePort("DeltaTime", false)] public float deltaTime;
    [NodePort("Time", false)] public float time;
    [NodePort("RealTime", false)] public float realTime;

    public override Func<GraphContext, ExecutionResult> Compile()
    {
        int dtId = GetOutputId("DeltaTime");
        int tId = GetOutputId("Time");
        int rtId = GetOutputId("RealTime");

        return (ctx) => {
            Write(ctx, dtId, Time.deltaTime);
            Write(ctx, tId, Time.time);
            Write(ctx, rtId, Time.realtimeSinceStartup);
            return ExecutionResult.Continue(-1);
        };
    }
}