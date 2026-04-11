using NodeEngine.Compilation;
using System;
using UnityEngine;

[NodePath("Engine/Time")]
public class TimeNode : BaseNode
{
    [NodePort("DeltaTime", false)] public float deltaTime;
    [NodePort("Time", false)] public float time;
    [NodePort("RealTime", false)] public float realTime;

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int dtId = context.GetOutputId("DeltaTime");
        int tId = context.GetOutputId("Time");
        int rtId = context.GetOutputId("RealTime");

        return (ctx) => {
            ctx.Write(dtId, Time.deltaTime);
            ctx.Write(tId, Time.time);
            ctx.Write(rtId, Time.realtimeSinceStartup);
            return ExecutionResult.Continue(-1);
        };
    }
}