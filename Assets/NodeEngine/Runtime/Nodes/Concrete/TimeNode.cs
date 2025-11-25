using System.Collections.Generic;

[NodePath("Engine/Time")]
public class TimeNode : NodeBase
{
    [NodeValue("DeltaTime", typeof(float))]
    public void DeltaTime(ConnectorValueFloat value)
    {
        value.SetValue(UnityEngine.Time.deltaTime);
    }

    [NodeValue("Time", typeof(float))]
    public void Time(ConnectorValueFloat value)
    {
        value.SetValue(UnityEngine.Time.time);
    }

    [NodeValue("RealTime", typeof(float))]
    public void RealTime(ConnectorValueFloat value)
    {
        value.SetValue(UnityEngine.Time.realtimeSinceStartup);
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>(false).SetHandler(DeltaTime).SetDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(false).SetHandler(Time).SetDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(false).SetHandler(RealTime).SetDefaultValue(new ConnectorValueFloat(0))
        };
    }

    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);
    }
}