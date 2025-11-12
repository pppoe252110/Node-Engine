using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

public class TimeNode : NodeBase
{
    [NodeValue("DeltaTime", typeof(float), KnownColor.LawnGreen)]
    public void DeltaTime(ConnectorValueSingle value)
    {
        value.SetValue(UnityEngine.Time.deltaTime);
    }

    [NodeValue("Time", typeof(float), KnownColor.LawnGreen)]
    public void Time(ConnectorValueSingle value)
    {
        value.SetValue(UnityEngine.Time.time);
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorValueSingle>(false).SetFunc(DeltaTime).ProvideDefaultValue(new ConnectorValueSingle(0)),
            new NodeField<ConnectorValueSingle>(false).SetFunc(Time).ProvideDefaultValue(new ConnectorValueSingle(0))
        };
    }

    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);
    }
}