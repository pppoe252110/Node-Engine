using System.Collections.Generic;

[NodePath("Engine/Time")]
public class TimeNode : NodeBase
{
    private ConnectorValueFloat _deltaTime;
    private ConnectorValueFloat _time;
    private ConnectorValueFloat _realTime;

    [NodeValue("DeltaTime", typeof(float))]
    public void DeltaTime(ConnectorValueFloat value)
    {
        _deltaTime.SetInnerValue(UnityEngine.Time.deltaTime);
    }

    [NodeValue("Time", typeof(float))]
    public void Time(ConnectorValueFloat value)
    {
        _time.SetInnerValue(UnityEngine.Time.time);
    }

    [NodeValue("RealTime", typeof(float))]
    public void RealTime(ConnectorValueFloat value)
    {
        _realTime.SetInnerValue(UnityEngine.Time.realtimeSinceStartup);
    }

    public override void Setup()
    {
        _deltaTime = new(0);
        _time = new(0);
        _realTime = new(0);

        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>(false).SetHandler(DeltaTime).SetDefaultValue(_deltaTime),
            new NodeField<ConnectorValueFloat>(false).SetHandler(Time).SetDefaultValue(_time),
            new NodeField<ConnectorValueFloat>(false).SetHandler(RealTime).SetDefaultValue(_realTime)
        };
    }

    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);
    }
}